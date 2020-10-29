/// \file MemorySerializer.cpp
/// \brief Contains definitions of classes and functions for processing data in
/// memory.
/// \bug No known bugs.

#include "MemorySerializer.hpp"

#include <QtEndian>

/// A namespace that contains common classes and functions for data
/// serialization.
namespace Common::Serialization {

    /// Constructs a memory serializer.
    /// \details Constructs a memory serializer that has no I/O device.
    MemorySerializer::MemorySerializer()
        : swapBytes_(QSysInfo::ByteOrder != QSysInfo::BigEndian),
          ownDevice_(false),
          status_(Status::Ok),
          endianness_(Endianness::BigEndian),
          device_(nullptr) {

    }

    /// Constructs a memory serializer.
    /// \details Constructs a memory serializer that uses the I/O \a device.
    /// \param[in]  device  I/O device.
    MemorySerializer::MemorySerializer(QIODevice* device)
        : swapBytes_(QSysInfo::ByteOrder != QSysInfo::BigEndian),
          ownDevice_(false),
          status_(Status::Ok),
          endianness_(Endianness::BigEndian),
          device_(device) {

    }

    /// Constructs a memory serializer.
    /// \details Constructs a memory serializer that operates on a byte array.
    /// The \a mode describes how the device is to be used.
    /// \param[in]  array   Byte array.
    /// \param[in]  mode    I/O mode.
    MemorySerializer::MemorySerializer(QByteArray* array,
                                       QIODevice::OpenMode mode)
        : swapBytes_(QSysInfo::ByteOrder != QSysInfo::BigEndian),
          ownDevice_(true),
          status_(Status::Ok),
          endianness_(Endianness::BigEndian),
          device_(new QBuffer(array)) {

        device_->open(mode);
    }

    /// Constructs a memory serializer.
    /// \details Constructs a read-only memory serializer that operates on a
    /// byte array.
    /// \param[in]  array   Byte array.
    MemorySerializer::MemorySerializer(const QByteArray& array)
        : swapBytes_(QSysInfo::ByteOrder != QSysInfo::BigEndian),
          ownDevice_(true),
          status_(Status::Ok),
          endianness_(Endianness::BigEndian),
          device_(new QBuffer) {

        static_cast<QBuffer*>(device_)->setData(array);
        static_cast<QBuffer*>(device_)->open(QIODevice::ReadOnly);
    }

    /// Destroys the memory serializer.
    /// \details The destructor will not affect the current I/O device, unless
    /// it is an internal device processing an array passed in the constructor,
    /// in which case the internal I/O device is destroyed.
    MemorySerializer::~MemorySerializer() {
        if (ownDevice_)
            delete device_;
    }

    /// Reads a signed byte from the serializer into \a item.
    /// \details Reads a signed byte from the serializer into \a item without
    /// any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(qint8& item) {
        constexpr auto size = sizeof (item);

        if (readRawData(reinterpret_cast<char*>(&item), size) != size)
            item = 0;

        return *this;
    }

    /// Reads an unsigned byte from the serializer into \a item.
    /// \details Reads an unsigned byte from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(quint8& item) {
        return *this >> reinterpret_cast<qint8&>(item);
    }

    /// Reads a signed 16-bit integer from the serializer into \a item.
    /// \details Reads a signed 16-bit integer from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(qint16& item) {
        constexpr auto size = sizeof (item);

        if (readRawData(reinterpret_cast<char*>(&item), size) != size)
            item = 0;
        else if (swapBytes_)
            item = qbswap(item);

        return *this;
    }

    /// Reads an unsigned 16-bit integer from the serializer into \a item.
    /// \details Reads an unsigned 16-bit integer from the serializer into
    /// \a item without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(quint16& item) {
        return *this >> reinterpret_cast<qint16&>(item);
    }

    /// Reads a signed 32-bit integer from the serializer into \a item.
    /// \details Reads a signed 32-bit integer from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(qint32& item) {
        constexpr auto size = sizeof (item);

        if (readRawData(reinterpret_cast<char*>(&item), size) != size)
            item = 0;
        else if (swapBytes_)
            item = qbswap(item);

        return *this;
    }

    /// Reads an unsigned 32-bit integer from the serializer into \a item.
    /// \details Reads an unsigned 32-bit integer from the serializer into
    /// \a item without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(quint32& item) {
        return *this >> reinterpret_cast<qint32&>(item);
    }

    /// Reads a signed 64-bit integer from the serializer into \a item.
    /// \details Reads a signed 64-bit integer from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(qint64& item) {
        constexpr auto size = sizeof (item);

        if (readRawData(reinterpret_cast<char*>(&item), size) != size)
            item = 0;
        else if (swapBytes_)
            item = qbswap(item);

        return *this;
    }

    /// Reads an unsigned 64-bit integer from the serializer into \a item.
    /// \details Reads an unsigned 64-bit integer from the serializer into
    /// \a item without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(quint64& item) {
        return *this >> reinterpret_cast<qint64&>(item);
    }

    /// Reads a floating point number from the serializer into \a item.
    /// \details Reads a floating point number from the serializer into \a item
    /// using the standard IEEE 754 format without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(qfloat16& item) {
        return *this >> reinterpret_cast<qint16&>(item);
    }

    /// Reads a boolean value from the serializer into \a item.
    /// \details Reads a boolean value from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(bool& item) {
        qint8 value;
        *this >> value;
        item = !!value;

        return *this;
    }

    /// Reads a floating point number from the serializer into \a item.
    /// \details Reads a floating point number from the serializer into \a item
    /// using the standard IEEE 754 format without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(float& item) {
        constexpr auto size = sizeof (item);

        if (readRawData(reinterpret_cast<char*>(&item), size) != size)
            item = 0.0f;
        else if (swapBytes_)
            item = qbswap(item);

        return *this;
    }

    /// Reads a floating point number from the serializer into \a item.
    /// \details Reads a floating point number from the serializer into \a item
    /// using the standard IEEE 754 format without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(double& item) {
        constexpr auto size = sizeof (item);

        if (readRawData(reinterpret_cast<char*>(&item), size) != size)
            item = 0.0;
        else if (swapBytes_)
            item = qbswap(item);

        return *this;
    }

    /// Reads a 16-bit character from the serializer into \a item.
    /// \details Reads a 16-bit character from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(char16_t& item) {
        quint16 value;
        *this >> value;
        item = static_cast<char16_t>(value);

        return *this;
    }

    /// Reads a 32-bit character from the serializer into \a item.
    /// \details Reads a 32-bit character from the serializer into \a item
    /// without any decoding.
    /// \param[out] item    The reference to the item to write to.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator>>(char32_t& item) {
        quint32 value;
        *this >> value;
        item = static_cast<char32_t>(value);

        return *this;
    }

    /// Writes a signed byte, \a item, to the serializer.
    /// \details Writes a signed byte, \a item, to the serializer without any
    /// encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(qint8 item) {
        writeRawData(reinterpret_cast<char*>(&item), sizeof (item));

        return *this;
    }

    /// Writes an unsigned byte, \a item, to the serializer.
    /// \details Writes an unsigned byte, \a item, to the serializer without
    /// any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(quint8 item) {
        return *this << static_cast<qint8>(item);
    }

    /// Writes a signed 16-bit integer, \a item, to the serializer.
    /// \details Writes a signed 16-bit integer, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(qint16 item) {
        if (swapBytes_) item = qbswap(item);

        writeRawData(reinterpret_cast<char*>(&item), sizeof (item));

        return *this;
    }

    /// Writes an unsigned 16-bit integer, \a item, to the serializer.
    /// \details Writes an unsigned 16-bit integer, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(quint16 item) {
        return *this << static_cast<qint16>(item);
    }

    /// Writes a signed 32-bit integer, \a item, to the serializer.
    /// \details Writes a signed 32-bit integer, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(qint32 item) {
        if (swapBytes_) item = qbswap(item);

        writeRawData(reinterpret_cast<char*>(&item), sizeof (item));

        return *this;
    }

    /// Writes an unsigned 32-bit integer, \a item, to the serializer.
    /// \details Writes an unsigned 32-bit integer, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(quint32 item) {
        return *this << static_cast<qint32>(item);
    }

    /// Writes a signed 64-bit integer, \a item, to the serializer.
    /// \details Writes a signed 64-bit integer, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(qint64 item) {
        if (swapBytes_) item = qbswap(item);

        writeRawData(reinterpret_cast<char*>(&item), sizeof (item));

        return *this;
    }

    /// Writes an unsigned 64-bit integer, \a item, to the serializer.
    /// \details Writes an unsigned 64-bit integer, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(quint64 item) {
        return *this << static_cast<qint64>(item);
    }

    /// Writes a floating point number, \a item, to the serializer.
    /// \details Writes a floating point number, \a item, to the serializer
    /// using the standard IEEE 754 format without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(qfloat16 item) {
        return *this << reinterpret_cast<qint16&>(item);
    }

    /// Writes a boolean value, \a item, to the serializer.
    /// \details Writes a boolean value, \a item, to the serializer
    /// without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(bool item) {
        return *this << static_cast<qint8>(item);
    }

    /// Writes a floating point number, \a item, to the serializer.
    /// \details Writes a floating point number, \a item, to the serializer
    /// using the standard IEEE 754 format without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(float item) {
        if (swapBytes_) item = qbswap(item);

        writeRawData(reinterpret_cast<char*>(&item), sizeof (item));

        return *this;
    }

    /// Writes a floating point number, \a item, to the serializer.
    /// \details Writes a floating point number, \a item, to the serializer
    /// using the standard IEEE 754 format without any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(double item) {
        if (swapBytes_) item = qbswap(item);

        writeRawData(reinterpret_cast<char*>(&item), sizeof (item));

        return *this;
    }

    /// Writes a 16-bit character, \a item, to the serializer.
    /// \details Writes a 16-bit character, \a item, to the serializer without
    /// any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(char16_t item) {
        return *this << static_cast<qint16>(item);
    }

    /// Writes a 32-bit character, \a item, to the serializer.
    /// \details Writes a 32-bit character, \a item, to the serializer without
    /// any encoding.
    /// \param[in]  item    The item to write.
    /// \return Reference to the serializer.
    MemorySerializer& MemorySerializer::operator<<(char32_t item) {
        return *this << static_cast<qint32>(item);
    }

    /// Reads at most \a length bytes from the serializer into \a buffer.
    /// \details Reads at most \a length bytes from the serializer into
    /// \a buffer without any decoding.
    /// \param[in]  buffer      Buffer to read to.
    /// \param[in]  bufferSize  Number of bytes to read.
    /// \return Number of bytes actually read, or -1 on error.
    int MemorySerializer::readRawData(char* buffer, int length) {
        if (!device_) return -1;

        const int result = device_->read(buffer, length);

        if (result != length)
            setStatus(Status::ReadPastEnd);

        return result;
    }

    /// Writes \a length bytes from \a buffer to the serializer.
    /// \details Writes \a length bytes from \a buffer to the serializer
    /// without any encoding.
    /// \param[in]  buffer      Buffer for writing.
    /// \param[in]  bufferSize  Number of bytes to write.
    /// \return Number of bytes actually written, or -1 on error.
    int MemorySerializer::writeRawData(const char* buffer, int length) {
        if (!device_ || status_ != Status::Ok) return -1;

        const int result = device_->write(buffer, length);

        if (result != length)
            setStatus(Status::WriteFailed);

        return result;
    }

    /// Skips \a length bytes from the device.
    /// \details This is equivalent to calling readRawData() on a buffer of
    /// \a length and ignoring the buffer.
    /// \param[in]  length  Number of bytes to skip.
    /// \return Number of bytes actually skipped, or -1 on error.
    int MemorySerializer::skipRawData(int length) {
        if (!device_) return -1;

        const int result = device_->skip(length);

        if (result != length)
            setStatus(Status::ReadPastEnd);

        return result;
    }

    /// Returns the I/O device.
    /// \details Returns the current I/O device from which data is written or
    /// read.
    /// \return I/O device.
    QIODevice* MemorySerializer::device() const {
        return device_;
    }

    /// Sets the I/O device.
    /// \details Sets the I/O device to \a device, which can be \c nullptr
    /// to unset to current I/O device.
    /// \param[in]  device  I/O device.
    void MemorySerializer::setDevice(QIODevice* device) {
        if (ownDevice_) {
            delete device_;
            ownDevice_ = false;
        }

        device_ = device;
    }

    /// Returns the number of bytes that are available for reading.
    /// \details Returns the number of bytes remaining available for reading.
    /// \return The number of bytes available for reading.
    qint64 MemorySerializer::bytesAvailable() const {
        return device_ ? device_->bytesAvailable() : 0;
    }

    /// Returns the position of the serializer.
    /// \details For random-access devices, this function returns the position
    /// that data is written to or read from. For sequential devices, 0 is
    /// returned.
    /// \return Position of the serializer.
    qint64 MemorySerializer::position() const {
        return device_ ? device_->pos() : 0;
    }

    /// Sets the position of the serializer to the \a position given.
    /// \details For random-access devices, this function sets the current
    /// position to \a position. For sequential devices, nothing changes.
    /// \param[in]  position    Serializer position.
    /// \retval \c true on success.
    /// \retval \c false on error.
    bool MemorySerializer::seek(qint64 position) {
        return device_ ? device_->seek(position) : false;
    }

    /// Indicates whether the I/O device has reached the end position.
    /// \details Indicates whether the I/O device has reached the end position
    /// (end of the stream or file) or if there is no I/O device set.
    /// \retval \c true if the I/O device has reached the end position.
    /// \retval \c false if the I/O device has not reached the end position.
    bool MemorySerializer::atEnd() const {
        return device_ ? device_->atEnd() : true;
    }

    /// Returns the status of the serializer.
    /// \details Returns the current status of the serializer.
    /// \return Status of the serializer.
    MemorySerializer::Status MemorySerializer::status() const {
        return status_;
    }

    /// Sets the status of the serializer to the \a status given.
    /// \details Subsequent calls are ignored until resetStatus() is called.
    /// \param[in]  status  Serializer status.
    void MemorySerializer::setStatus(Status status) {
        if (status_ == Status::Ok)
            status_ = status;
    }

    /// Resets the status of the serializer.
    /// \details Resets the status of the serializer to the default value.
    void MemorySerializer::resetStatus() {
        status_ = Status::Ok;
    }

    /// Returns the current data endianness.
    /// \details Returns the current data endianness of either big-endian or
    /// little-endian.
    /// \return Current data endianness.
    MemorySerializer::Endianness MemorySerializer::endianness() const {
        return endianness_;
    }

    /// Sets the data endianness to \a endianness.
    /// \details Sets the data endianness of either big-endian or little-endian.
    /// \param[in]  endianness  Data endianness.
    void MemorySerializer::setEndianness(Endianness endianness) {
        endianness_ = endianness;

        if (QSysInfo::ByteOrder == QSysInfo::BigEndian)
            swapBytes_ = endianness_ != Endianness::BigEndian;
        else
            swapBytes_ = endianness_ != Endianness::LittleEndian;
    }
}
