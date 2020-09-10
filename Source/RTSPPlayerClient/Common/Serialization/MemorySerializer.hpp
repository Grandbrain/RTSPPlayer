/// \file MemorySerializer.hpp
/// \brief Contains declarations of classes and functions for processing data in
/// memory.
/// \bug No known bugs.

#ifndef MEMORYSERIALIZER_HPP
#define MEMORYSERIALIZER_HPP

#include <QBuffer>
#include <QFloat16>

/// A namespace that contains common classes and functions for data
/// serialization.
namespace Common::Serialization {

    /// A class that provides a memory serializer implementation for
    /// reading/writing the data.
    class MemorySerializer {

        Q_DISABLE_COPY(MemorySerializer)

    public:

        /// An enumeration that describes the byte order used for
        /// reading/writing the data.
        enum class ByteOrder {
            BigEndian   , ///< Most significant byte first.
            LittleEndian, ///< Least significant byte first.
        };

        /// An enumeration that describes the current status of the serializer.
        enum class Status {
            Ok          , ///< The serializer is operating normally.
            ReadPastEnd , ///< The serializer has read past the end of the data.
            WriteFailed , ///< The serializer cannot write to the underlying device.
        };

    public:

        /// Constructs a memory serializer.
        explicit MemorySerializer();

        /// Constructs a memory serializer.
        /// \param[in]  device  I/O device.
        explicit MemorySerializer(QIODevice* device);

        /// Constructs a memory serializer.
        /// \param[in]  array   Byte array.
        /// \param[in]  mode    I/O mode.
        explicit MemorySerializer(QByteArray* array, QIODevice::OpenMode mode);

        /// Constructs a memory serializer.
        /// \param[in]  array   Byte array.
        explicit MemorySerializer(const QByteArray& array);

        /// Destroys the memory serializer.
        virtual ~MemorySerializer();

    public:

        /// Reads a signed byte from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(qint8& item);

        /// Reads an unsigned byte from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(quint8& item);

        /// Reads a signed 16-bit integer from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(qint16& item);

        /// Reads an unsigned 16-bit integer from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(quint16& item);

        /// Reads a signed 32-bit integer from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(qint32& item);

        /// Reads an unsigned 32-bit integer from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(quint32& item);

        /// Reads a signed 64-bit integer from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(qint64& item);

        /// Reads an unsigned 64-bit integer from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(quint64& item);

        /// Reads a floating point number from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(qfloat16& item);

        /// Reads a boolean value from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(bool& item);

        /// Reads a floating point number from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(float& item);

        /// Reads a floating point number from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(double& item);

        /// Reads a 16-bit character from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(char16_t& item);

        /// Reads a 32-bit character from the serializer into \a item.
        /// \param[out] item    The reference to the item to write to.
        /// \return Reference to the serializer.
        MemorySerializer& operator>>(char32_t& item);

        /// Writes a signed byte, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(qint8 item);

        /// Writes an unsigned byte, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(quint8 item);

        /// Writes a signed 16-bit integer, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(qint16 item);

        /// Writes an unsigned 16-bit integer, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(quint16 item);

        /// Writes a signed 32-bit integer, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(qint32 item);

        /// Writes an unsigned 32-bit integer, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(quint32 item);

        /// Writes a signed 64-bit integer, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(qint64 item);

        /// Writes an unsigned 64-bit integer, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(quint64 item);

        /// Writes a floating point number, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(qfloat16 item);

        /// Writes a boolean value, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(bool item);

        /// Writes a floating point number, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(float item);

        /// Writes a floating point number, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(double item);

        /// Writes a 16-bit character, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(char16_t item);

        /// Writes a 32-bit character, \a item, to the serializer.
        /// \param[in]  item    The item to write.
        /// \return Reference to the serializer.
        MemorySerializer& operator<<(char32_t item);

    public:

        /// Reads at most \a length bytes from the serializer into \a buffer.
        /// \param[in]  buffer      Buffer to read to.
        /// \param[in]  bufferSize  Number of bytes to read.
        /// \return Number of bytes actually read, or -1 on error.
        int readRawData(char* buffer, int length);

        /// Writes \a length bytes from \a buffer to the serializer.
        /// \param[in]  buffer      Buffer for writing.
        /// \param[in]  bufferSize  Number of bytes to write.
        /// \return Number of bytes actually written, or -1 on error.
        int writeRawData(const char* buffer, int length);

        /// Skips \a length bytes from the device.
        /// \param[in]  length  Number of bytes to skip.
        /// \return Number of bytes actually skipped, or -1 on error.
        int skipRawData(int length);

        /// Returns the I/O device.
        /// \return I/O device.
        QIODevice* device() const;

        /// Sets the I/O device.
        /// \param[in]  device  I/O device.
        void setDevice(QIODevice* device);

        /// Returns the number of bytes that are available for reading.
        /// \return The number of bytes available for reading.
        qint64 bytesAvailable() const;

        /// Returns the position of the serializer.
        /// \return Position of the serializer.
        qint64 position() const;

        /// Sets the position of the serializer to the \a position given.
        /// \param[in]  position    Serializer position.
        /// \retval \c true on success.
        /// \retval \c false on error.
        bool seek(qint64 position);

        /// Indicates whether the I/O device has reached the end position.
        /// \retval \c true if the I/O device has reached the end position.
        /// \retval \c false if the I/O device has not reached the end position.
        bool atEnd() const;

        /// Returns the status of the serializer.
        /// \return Status of the serializer.
        Status status() const;

        /// Sets the status of the serializer to the \a status given.
        /// \param[in]  status  Serializer status.
        void setStatus(Status status);

        /// Resets the status of the serializer.
        void resetStatus();

        /// Returns the current byte order setting.
        /// \return Current byte order.
        ByteOrder byteOrder() const;

        /// Sets the byte order setting to \a byteOrder.
        /// \param[in]  byteOrder   Byte order.
        void setByteOrder(ByteOrder byteOrder);

    private:

        /// Indicates whether the serializer should swap bytes according to the
        /// byte order.
        bool swapBytes_;

        /// Indicates whether the serializer owns the I/O device.
        bool ownDevice_;

        /// Status.
        Status status_;

        /// Byte order.
        ByteOrder byteOrder_;

        /// I/O device.
        QIODevice* device_;
    };
}

#endif
