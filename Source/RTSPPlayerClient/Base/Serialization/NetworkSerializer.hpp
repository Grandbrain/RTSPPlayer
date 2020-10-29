/// \file NetworkSerializer.hpp
/// \brief Contains declarations of classes and functions for processing network
/// data.
/// \bug No known bugs.

#ifndef NETWORKSERIALIZER_HPP
#define NETWORKSERIALIZER_HPP

#include "MemorySerializer.hpp"

#include <QHash>
#include <QByteArray>

/// A namespace that contains common classes and functions for data
/// serialization.
namespace Common::Serialization {

    /// A structure that defines a network frame.
    struct NetworkFrame {

        /// Frame identifier.
        quint32 id = 0;

        /// Frame number.
        quint16 number = 0;

        /// Frame interpretation.
        quint8 interpretation = 0;

        /// Frame processing time.
        quint16 time = 0;

        /// Frame priority.
        quint8 priority = 10;

        /// Sender task identifier.
        QString task;

        /// Information flow identifier.
        QString flow;

        /// Frame data array.
        QByteArray data;
    };

    /// A class that provides a network frame builder implementation.
    class NetworkFrameBuilder {
    public:

        /// Constructs a network frame builder.
        explicit NetworkFrameBuilder() = default;

        /// Destroys the network frame builder.
        virtual ~NetworkFrameBuilder() = default;

    public:

        /// Indicates whether the frame is fully collected.
        /// \retval \c true if the frame is fully collected.
        /// \retval \c false the frame is not fully collected.
        bool isFrameCompleted() const;

        /// Returns the collected frame.
        /// \return Collected frame.
        const NetworkFrame& getFrame() const;

        /// Returns the collected frame.
        /// \return Collected frame.
        NetworkFrame& getFrame();

        /// Puts a master chunk to the frame.
        /// \param[in]  frameSize       Frame size.
        /// \param[in]  partialFrame    Chunk data.
        /// \retval \c true on success.
        /// \retval \c false on error.
        bool putMasterChunk(int frameSize, const NetworkFrame& partialFrame);

        /// Puts a slave chunk to the frame.
        /// \param[in]  frameOffset     Offset in frame data.
        /// \param[in]  partialFrame    Chunk data.
        /// \retval \c true on success.
        /// \retval \c false on error.
        bool putSlaveChunk(int frameOffset, const NetworkFrame& partialFrame);

    private:

        /// Calculates the number of chunks by frame size \a frameSize.
        /// \param[in]  frameSize   Frame size.
        /// \return Number of chunks.
        static int getChunkNumber(int frameSize);

    private:

        /// Indicates whether the master chunk is found.
        bool masterChunkFound_ = false;

        /// Number of collected chunks.
        int collectedChunks_ = 0;

        /// Number of detected chunks.
        int detectedChunks_ = 0;

        /// Network frame.
        NetworkFrame frame_;
    };

    /// A class that provides a network serializer implementation.
    class NetworkSerializer {
    public:

        /// Constructs a network serializer.
        explicit NetworkSerializer();

        /// Constructs a network serializer.
        /// \param[in]  endianness  Data endianness.
        explicit NetworkSerializer(MemorySerializer::Endianness endianness);

        /// Destroys the network serializer.
        virtual ~NetworkSerializer() = default;

    public:

        /// Returns the data endianness.
        /// \return Data endianness.
        MemorySerializer::Endianness endianness() const;

        /// Serializes the network frame into datagrams.
        /// \param[in]  frame   Network frame.
        /// \return List of datagrams.
        std::list<QByteArray> serialize(const NetworkFrame& frame) const;

        /// Serializes the network frame into datagrams.
        /// \param[in]  frame       Network frame.
        /// \param[out] datagrams   List of datagrams.
        void serialize(const NetworkFrame& frame,
                       std::list<QByteArray>& datagrams) const;

        /// Deserializes a datagram to collect frames.
        /// \param[in]  data    Datagram data to parse.
        /// \param[in]  size    Datagram data size.
        void deserialize(const char* data, int size);

        /// Deserializes a datagram to collect frames.
        /// \param[in]  datagram    Datagram to parse.
        void deserialize(const QByteArray& datagram);

        /// Returns completed frames.
        /// \return List of frames.
        std::list<NetworkFrame> completedFrames();

        /// Returns completed frames.
        /// \param[out] frames  List of frames.
        void completedFrames(std::list<NetworkFrame>& frames);

        /// Clears pending frames.
        void clear();

    private:

        /// Data endianness.
        MemorySerializer::Endianness endianness_;

        /// A container for the collected frames.
        QHash<quint32, NetworkFrameBuilder> collectedFrames_;
    };
}

#endif // NETWORKSERIALIZER_HPP
