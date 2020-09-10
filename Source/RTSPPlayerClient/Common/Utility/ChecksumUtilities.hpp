/// \file ChecksumUtilities.hpp
/// \brief Contains declarations of utility classes and functions for
/// calculating checksums.
/// \bug No known bugs.

#ifndef CHECKSUMUTILITIES_HPP
#define CHECKSUMUTILITIES_HPP

#include <QByteArray>

/// A namespace that contains common utility classes and functions.
namespace Common::Utility {

    /// Calculates CRC-16 for a data array.
    /// \param[in]  data    Data array.
    /// \param[in]  size    Data size.
    /// \param[in]  skip    Array indices to skip.
    /// \return CRC-16 value.
    quint16 crc16(const char* data,
                  int size,
                  std::initializer_list<int> skip = { }) noexcept;

    /// Calculates CRC-16 for a data array.
    /// \param[in]  array   Byte array.
    /// \param[in]  skip    Array indices to skip.
    /// \return CRC-16 value.
    quint16 crc16(const QByteArray& array,
                  std::initializer_list<int> skip = { }) noexcept;
}

#endif
