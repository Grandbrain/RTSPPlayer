/// \file ChronoUtilities.cpp
/// \brief Contains definitions of utility classes and functions for calculating
/// timestamps.
/// \bug No known bugs.

#include "ChronoUtilities.hpp"

#include <chrono>

/// A namespace that contains common utility classes and functions.
namespace Common::Utility {

    /// Generates a 64-bit timestamp from microseconds.
    /// \details Generates a 64-bit timestamp using steady clock.
    /// \return A 64-bit timestamp from microseconds.
    quint64 timestampMicroseconds64() noexcept {
        static auto base =
            std::chrono::time_point_cast<std::chrono::microseconds>(
                std::chrono::steady_clock::now());

        auto current =
            std::chrono::time_point_cast<std::chrono::microseconds>(
                std::chrono::steady_clock::now());

        if (current <= base) base += std::chrono::microseconds(1);
        else base = current;

        return base.time_since_epoch().count();
    }

    /// Generates a 32-bit timestamp from microseconds.
    /// \details Generates a 32-bit timestamp using steady clock.
    /// \return A 32-bit timestamp from microseconds.
    quint32 timestampMicroseconds32() noexcept {
        return static_cast<quint32>(timestampMicroseconds64());
    }
}
