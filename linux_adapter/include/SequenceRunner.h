#pragma once

#include "SequenceTypes.h"
#include "LSAdapter.h"

class SequenceRunner
{
public:
    SequenceRunner();

    bool LoadFromFile(const std::string& path, std::string& error);
    bool Run(bool simulationMode, std::string& error);

private:
    Sequence sequence_;
    LSAdapter adapter_;

    bool ExecuteStep(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteMotion(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteWait(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteLinearMove(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteRelMove(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteCircularMove(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteCounter(const Step& step, bool simulationMode, std::string& error);
    bool ExecuteFlow(const Step& step, bool simulationMode, std::string& error);
};
