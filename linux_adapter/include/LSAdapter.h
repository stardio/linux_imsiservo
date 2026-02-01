#pragma once

#include <string>

class LSAdapter
{
public:
    LSAdapter();
    ~LSAdapter();

    bool Load(const std::string& soPath, std::string& error);
    void Unload();

    bool IsLoaded() const;

    bool MoveAbs(const std::string& axis, double pos, double speed, std::string& error);
    bool MoveRel(const std::string& axis, double distance, double speed, std::string& error);
    bool MoveLinear(double x, double y, double z, double speed, std::string& error);
    bool MoveCircular(double cx, double cy, double ex, double ey, const std::string& dir, std::string& error);
    bool WaitMs(int delayMs, std::string& error);

private:
    void* handle_ = nullptr;

    using InitFn = int (*)();
    using ShutdownFn = int (*)();
    using AbsMoveFn = int (*)(const char*, double, double);
    using RelMoveFn = int (*)(const char*, double, double);
    using LinearMoveFn = int (*)(double, double, double, double);
    using CircularMoveFn = int (*)(double, double, double, double, const char*);
    using WaitFn = int (*)(int);

    InitFn init_ = nullptr;
    ShutdownFn shutdown_ = nullptr;
    AbsMoveFn absMove_ = nullptr;
    RelMoveFn relMove_ = nullptr;
    LinearMoveFn linearMove_ = nullptr;
    CircularMoveFn circularMove_ = nullptr;
    WaitFn wait_ = nullptr;

    bool ResolveSymbols(std::string& error);
};
