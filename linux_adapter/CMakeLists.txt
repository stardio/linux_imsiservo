cmake_minimum_required(VERSION 3.16)
project(ethercat_sequence_adapter LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

include(FetchContent)
FetchContent_Declare(
  nlohmann_json
  URL https://github.com/nlohmann/json/releases/download/v3.11.3/json.tar.xz
)
FetchContent_MakeAvailable(nlohmann_json)

add_executable(ethercat_sequence_adapter
  src/main.cpp
  src/LSAdapter.cpp
  src/SequenceRunner.cpp
)

target_include_directories(ethercat_sequence_adapter PRIVATE include)

target_link_libraries(ethercat_sequence_adapter PRIVATE nlohmann_json::nlohmann_json)

if(UNIX)
  target_link_libraries(ethercat_sequence_adapter PRIVATE dl)
endif()
