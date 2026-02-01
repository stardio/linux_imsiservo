#include "LSAdapter.h"

#include <cstdlib>

#ifdef __linux__
#include <dlfcn.h>
#endif

LSAdapter::LSAdapter() = default;

LSAdapter::~LSAdapter()
{
    Unload();
}

bool LSAdapter::Load(const std::string& soPath, std::string& error)
{
#ifdef __linux__
    handle_ = dlopen(soPath.c_str(), RTLD_LAZY);
    if (!handle_)
    {
        error = std::string("dlopen failed: ") + dlerror();
        return false;
    }

    if (!ResolveSymbols(error))
    {
        Unload();
        return false;
    }

    if (init_ && init_() != 0)
    {
        error = "LS init failed";
        Unload();
        return false;
    }

    return true;
#else
    (void)soPath;
    error = "LSAdapter only supported on Linux";
    return false;
#endif
}

void LSAdapter::Unload()
{
#ifdef __linux__
    if (shutdown_) shutdown_();
    shutdown_ = nullptr;
    init_ = nullptr;
    absMove_ = nullptr;
    relMove_ = nullptr;
    linearMove_ = nullptr;
    circularMove_ = nullptr;
    wait_ = nullptr;

    if (handle_)
    {
        dlclose(handle_);
        handle_ = nullptr;
    }
#endif
}

bool LSAdapter::IsLoaded() const
{
    return handle_ != nullptr;
}

bool LSAdapter::ResolveSymbols(std::string& error)
{
#ifdef __linux__
    auto get = [&](const char* name) -> void* {
        return dlsym(handle_, name);
    };

    // NOTE: These symbol names are placeholders. Adjust to match LS_SERVO_Controller exports.
    init_ = reinterpret_cast<InitFn>(get("LS_Init"));
    shutdown_ = reinterpret_cast<ShutdownFn>(get("LS_Shutdown"));
    absMove_ = reinterpret_cast<AbsMoveFn>(get("LS_MoveAbs"));
    relMove_ = reinterpret_cast<RelMoveFn>(get("LS_MoveRel"));
    linearMove_ = reinterpret_cast<LinearMoveFn>(get("LS_MoveLinear"));
    circularMove_ = reinterpret_cast<CircularMoveFn>(get("LS_MoveCircular"));
    wait_ = reinterpret_cast<WaitFn>(get("LS_WaitMs"));

    if (!absMove_ || !relMove_ || !linearMove_ || !circularMove_ || !wait_)
    {
        error = "Missing required symbols in LS_SERVO_Controller library";
        return false;
    }

    return true;
#else
    error = "Not supported";
    return false;
#endif
}

bool LSAdapter::MoveAbs(const std::string& axis, double pos, double speed, std::string& error)
{
#ifdef __linux__
    if (!absMove_)
    {
        error = "LS_MoveAbs not loaded";
        return false;
    }
    return absMove_(axis.c_str(), pos, speed) == 0;
#else
    (void)axis; (void)pos; (void)speed;
    error = "Not supported";
    return false;
#endif
}

bool LSAdapter::MoveRel(const std::string& axis, double distance, double speed, std::string& error)
{
#ifdef __linux__
    if (!relMove_)
    {
        error = "LS_MoveRel not loaded";
        return false;
    }
    return relMove_(axis.c_str(), distance, speed) == 0;
#else
    (void)axis; (void)distance; (void)speed;
    error = "Not supported";
    return false;
#endif
}

bool LSAdapter::MoveLinear(double x, double y, double z, double speed, std::string& error)
{
#ifdef __linux__
    if (!linearMove_)
    {
        error = "LS_MoveLinear not loaded";
        return false;
    }
    return linearMove_(x, y, z, speed) == 0;
#else
    (void)x; (void)y; (void)z; (void)speed;
    error = "Not supported";
    return false;
#endif
}

bool LSAdapter::MoveCircular(double cx, double cy, double ex, double ey, const std::string& dir, std::string& error)
{
#ifdef __linux__
    if (!circularMove_)
    {
        error = "LS_MoveCircular not loaded";
        return false;
    }
    return circularMove_(cx, cy, ex, ey, dir.c_str()) == 0;
#else
    (void)cx; (void)cy; (void)ex; (void)ey; (void)dir;
    error = "Not supported";
    return false;
#endif
}

bool LSAdapter::WaitMs(int delayMs, std::string& error)
{
#ifdef __linux__
    if (!wait_)
    {
        error = "LS_WaitMs not loaded";
        return false;
    }
    return wait_(delayMs) == 0;
#else
    (void)delayMs;
    error = "Not supported";
    return false;
#endif
}
