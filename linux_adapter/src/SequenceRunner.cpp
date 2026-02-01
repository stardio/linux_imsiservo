#include "SequenceRunner.h"

#include <chrono>
#include <fstream>
#include <iostream>
#include <thread>

using nlohmann::json;

SequenceRunner::SequenceRunner() = default;

bool SequenceRunner::LoadFromFile(const std::string& path, std::string& error)
{
    std::ifstream in(path);
    if (!in.is_open())
    {
        error = "Failed to open sequence file: " + path;
        return false;
    }

    json doc;
    try
    {
        in >> doc;
    }
    catch (const std::exception& ex)
    {
        error = std::string("JSON parse error: ") + ex.what();
        return false;
    }

    if (!doc.contains("sequence_name") || !doc.contains("steps"))
    {
        error = "Invalid sequence format: missing sequence_name or steps";
        return false;
    }

    sequence_.name = doc["sequence_name"].get<std::string>();
    sequence_.steps.clear();
    sequence_.indexById.clear();

    const auto& steps = doc["steps"];
    if (!steps.is_array())
    {
        error = "Invalid steps: expected array";
        return false;
    }

    for (const auto& s : steps)
    {
        Step step;
        step.id = s.value("id", "");
        step.type = s.value("type", "");
        step.params = s.value("params", json::object());
        if (s.contains("next") && !s["next"].is_null())
        {
            step.next = s["next"].get<std::string>();
        }
        else
        {
            step.next.clear();
        }

        if (step.id.empty())
        {
            error = "Invalid step: missing id";
            return false;
        }

        sequence_.indexById[step.id] = sequence_.steps.size();
        sequence_.steps.push_back(step);
    }

    return true;
}

bool SequenceRunner::Run(bool simulationMode, std::string& error)
{
    if (sequence_.steps.empty())
    {
        error = "Sequence is empty";
        return false;
    }

    if (!simulationMode)
    {
        const char* libPath = std::getenv("LS_SERVO_LIB");
        std::string soPath = libPath ? libPath : "libls_servo_controller.so";
        if (!adapter_.Load(soPath, error))
        {
            return false;
        }
    }

    std::string currentId = sequence_.steps.front().id;
    std::unordered_map<std::string, int> counterState;
    int guard = 0;
    const int guardMax = 100000;

    while (!currentId.empty())
    {
        if (++guard > guardMax)
        {
            error = "Execution guard triggered (possible infinite loop)";
            return false;
        }

        auto it = sequence_.indexById.find(currentId);
        if (it == sequence_.indexById.end())
        {
            error = "Step not found: " + currentId;
            return false;
        }

        const Step& step = sequence_.steps[it->second];
        if (!ExecuteStep(step, simulationMode, error))
        {
            return false;
        }

        if (step.type == "COUNTER")
        {
            const auto& p = step.params;
            std::string name = p.value("name", "");
            int initial = p.value("initial", 0);
            int target = p.value("target", 0);
            int increment = p.value("increment", 1);
            std::string gotoNode = p.value("gotoNode", "");

            if (name.empty())
            {
                error = "COUNTER missing name";
                return false;
            }

            int& value = counterState[name];
            if (value == 0 && initial != 0)
            {
                value = initial;
            }
            else
            {
                value += increment;
            }

            if (value < target && !gotoNode.empty())
            {
                currentId = gotoNode;
                continue;
            }
        }

        currentId = step.next;
    }

    return true;
}

bool SequenceRunner::ExecuteStep(const Step& step, bool simulationMode, std::string& error)
{
    if (step.type == "MOTION") return ExecuteMotion(step, simulationMode, error);
    if (step.type == "WAIT") return ExecuteWait(step, simulationMode, error);
    if (step.type == "LINEAR_MOVE") return ExecuteLinearMove(step, simulationMode, error);
    if (step.type == "REL_MOVE") return ExecuteRelMove(step, simulationMode, error);
    if (step.type == "CIRCULAR_MOVE") return ExecuteCircularMove(step, simulationMode, error);
    if (step.type == "COUNTER") return ExecuteCounter(step, simulationMode, error);
    if (step.type == "FLOW") return ExecuteFlow(step, simulationMode, error);
    if (step.type == "START" || step.type == "END" || step.type == "SYSTEM")
    {
        if (simulationMode)
        {
            std::cout << "[SIM] " << step.type << " " << step.id << std::endl;
        }
        return true;
    }

    error = "Unknown step type: " + step.type;
    return false;
}

bool SequenceRunner::ExecuteMotion(const Step& step, bool simulationMode, std::string& error)
{
    const auto& p = step.params;
    std::string axis = p.value("axis", "X");
    double pos = p.value("pos", 0.0);
    double speed = p.value("speed", 0.0);

    if (simulationMode)
    {
        std::cout << "[SIM] MOTION axis=" << axis << " pos=" << pos << " speed=" << speed << std::endl;
        return true;
    }

    return adapter_.MoveAbs(axis, pos, speed, error);
}

bool SequenceRunner::ExecuteWait(const Step& step, bool simulationMode, std::string& error)
{
    int delay = step.params.value("delay_ms", 0);

    if (simulationMode)
    {
        std::cout << "[SIM] WAIT " << delay << " ms" << std::endl;
        std::this_thread::sleep_for(std::chrono::milliseconds(delay));
        return true;
    }

    return adapter_.WaitMs(delay, error);
}

bool SequenceRunner::ExecuteLinearMove(const Step& step, bool simulationMode, std::string& error)
{
    const auto& p = step.params;
    auto target = p.value("target", json::object());
    double x = target.value("X", 0.0);
    double y = target.value("Y", 0.0);
    double z = target.value("Z", 0.0);
    double speed = p.value("speed", 0.0);

    if (simulationMode)
    {
        std::cout << "[SIM] LINEAR_MOVE X=" << x << " Y=" << y << " Z=" << z << " speed=" << speed << std::endl;
        return true;
    }

    return adapter_.MoveLinear(x, y, z, speed, error);
}

bool SequenceRunner::ExecuteRelMove(const Step& step, bool simulationMode, std::string& error)
{
    const auto& p = step.params;
    std::string axis = p.value("axis", "X");
    double distance = p.value("distance", 0.0);
    double speed = p.value("speed", 0.0);

    if (simulationMode)
    {
        std::cout << "[SIM] REL_MOVE axis=" << axis << " dist=" << distance << " speed=" << speed << std::endl;
        return true;
    }

    return adapter_.MoveRel(axis, distance, speed, error);
}

bool SequenceRunner::ExecuteCircularMove(const Step& step, bool simulationMode, std::string& error)
{
    const auto& p = step.params;
    auto center = p.value("center", json::object());
    auto end = p.value("end", json::object());
    double cx = center.value("X", 0.0);
    double cy = center.value("Y", 0.0);
    double ex = end.value("X", 0.0);
    double ey = end.value("Y", 0.0);
    std::string direction = p.value("direction", "CW");

    if (simulationMode)
    {
        std::cout << "[SIM] CIRCULAR_MOVE center(" << cx << "," << cy << ") end(" << ex << "," << ey << ") dir=" << direction << std::endl;
        return true;
    }

    return adapter_.MoveCircular(cx, cy, ex, ey, direction, error);
}

bool SequenceRunner::ExecuteCounter(const Step& step, bool simulationMode, std::string& error)
{
    if (simulationMode)
    {
        const auto& p = step.params;
        std::cout << "[SIM] COUNTER name=" << p.value("name", "")
                  << " initial=" << p.value("initial", 0)
                  << " target=" << p.value("target", 0)
                  << " inc=" << p.value("increment", 1)
                  << " goto=" << p.value("gotoNode", "")
                  << std::endl;
        return true;
    }

    // TODO: Integrate with live controller state
    (void)error;
    return true;
}

bool SequenceRunner::ExecuteFlow(const Step& step, bool simulationMode, std::string& error)
{
    if (simulationMode)
    {
        std::cout << "[SIM] FLOW params=" << step.params.dump() << std::endl;
        return true;
    }

    // TODO: Implement FLOW in live mode if needed
    (void)error;
    return true;
}
