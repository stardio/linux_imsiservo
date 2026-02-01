#include "SequenceRunner.h"

#include <iostream>

static void PrintUsage()
{
    std::cout << "Usage: ethercat_sequence_adapter --sequence <path> [--mode sim|live]" << std::endl;
}

int main(int argc, char** argv)
{
    std::string sequencePath;
    std::string mode = "sim";

    for (int i = 1; i < argc; ++i)
    {
        std::string arg = argv[i];
        if (arg == "--sequence" && i + 1 < argc)
        {
            sequencePath = argv[++i];
        }
        else if (arg == "--mode" && i + 1 < argc)
        {
            mode = argv[++i];
        }
    }

    if (sequencePath.empty())
    {
        PrintUsage();
        return 1;
    }

    bool simulation = (mode != "live");

    SequenceRunner runner;
    std::string error;

    if (!runner.LoadFromFile(sequencePath, error))
    {
        std::cerr << "Load failed: " << error << std::endl;
        return 2;
    }

    if (!runner.Run(simulation, error))
    {
        std::cerr << "Run failed: " << error << std::endl;
        return 3;
    }

    return 0;
}
