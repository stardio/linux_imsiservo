#pragma once

#include <string>
#include <unordered_map>
#include <vector>
#include <nlohmann/json.hpp>

struct Step
{
    std::string id;
    std::string type;
    nlohmann::json params;
    std::string next; // empty means null
};

struct Sequence
{
    std::string name;
    std::vector<Step> steps;
    std::unordered_map<std::string, size_t> indexById;
};
