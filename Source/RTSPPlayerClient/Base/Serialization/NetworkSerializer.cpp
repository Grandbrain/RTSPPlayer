/// \file NetworkSerializer.cpp
/// \brief Contains definitions of classes and functions for processing network
/// data.
/// \bug No known bugs.

#include "NetworkSerializer.hpp"
#include "Common/Utility/ChecksumUtilities.hpp"
#include "Common/Utility/ChronoUtilities.hpp"

namespace {

    /// Datagram protocol version.
    /// \details Specific protocol version to check data integrity.
    constexpr quint16 DATAGRAM_PROTOCOL_VERSION { 0x0100 };

    /// Master chunk identifier.
    /// \details Master chunk identifier code.
    constexpr quint8 CHUNK_MASTER_ID { 1 };

    /// Slave chunk identifier.
    /// \details Slave chunk identifier code.
    constexpr quint8 CHUNK_SLAVE_ID { 0 };

    /// Datagram header size.
    /// \details Datagram header size in bytes.
    constexpr int DATAGRAM_HEADER_SIZE { 10 };

    /// Master chunk header size.
    /// \details Master chunk header size in bytes.
    constexpr int CHUNK_MASTER_HEADER_SIZE { 29 };

#if defined (NETWORK_PROTOCOL_EXTENDED) && (NETWORK_PROTOCOL_EXTENDED == 1)
    /// Slave chunk header size.
    /// \details Slave chunk header size in bytes.
    constexpr int CHUNK_SLAVE_HEADER_SIZE { 29 };
#else
    /// Slave chunk header size.
    /// \details Slave chunk header size in bytes.
    constexpr int CHUNK_SLAVE_HEADER_SIZE { 25 };
#endif

    /// Chunk task identifier size.
    /// \details Chunk task identifier size in bytes.
    constexpr int CHUNK_TASK_SIZE { 6 };

    /// Chunk flow identifier size.
    /// \details Chunk flow identifier size in bytes.
    constexpr int CHUNK_FLOW_SIZE { 6 };

    /// Frame data maximum size.
    /// \details Frame maximum size without metadata in bytes.
    constexpr int FRAME_MAX_SIZE { 31'850'493 };

    /// Datagram maximum size.
    /// \details Datagram maximum size with metadata in bytes.
    constexpr int DATAGRAM_MAX_SIZE { 1500 };

    /// Chunk maximum size.
    /// \details Chunk maximum size with metadata in bytes.
    constexpr int CHUNK_MAX_SIZE { 512 };

    /// Datagram data maximum size.
    /// \details Datagram maximum size without metadata in bytes.
    constexpr int DATAGRAM_DATA_MAX_SIZE {
        DATAGRAM_MAX_SIZE - DATAGRAM_HEADER_SIZE
    };

    /// Master chunk data maximum size.
    /// \details Master chunk maximum size without metadata in bytes.
    constexpr int CHUNK_MASTER_DATA_MAX_SIZE {
        CHUNK_MAX_SIZE - CHUNK_MASTER_HEADER_SIZE
    };

    /// Slave chunk data maximum size.
    /// \details Slave chunk maximum size without metadata in bytes.
    constexpr int CHUNK_SLAVE_DATA_MAX_SIZE {
        CHUNK_MAX_SIZE - CHUNK_SLAVE_HEADER_SIZE
    };
}

/// A namespace that contains common classes and functions for data
/// serialization.
namespace Common::Serialization {

    /// Indicates whether the frame is fully collected.
    /// \details Compares the number of collected and detected chunks.
    /// \retval \c true if the frame is fully collected.
    /// \retval \c false the frame is not fully collected.
    bool NetworkFrameBuilder::isFrameCompleted() const {
        return detectedChunks_ != 0 && collectedChunks_ == detectedChunks_;
    }

    /// Returns the collected frame.
    /// \details Returns a reference to the collected frame.
    /// \return Collected frame.
    const NetworkFrame& NetworkFrameBuilder::getFrame() const {
        return frame_;
    }

    /// Returns the collected frame.
    /// \details Returns a reference to the collected frame.
    /// \return Collected frame.
    NetworkFrame& NetworkFrameBuilder::getFrame() {
        return frame_;
    }

    /// Puts a master chunk to the frame.
    /// \details Writes chunk data to the frame buffer. The \a frameSize
    /// parameter is necessary to calculate the total number of chunks.
    /// \param[in]  frameSize       Frame size.
    /// \param[in]  partialFrame    Chunk data.
    /// \retval \c true on success.
    /// \retval \c false on error.
    bool NetworkFrameBuilder::putMasterChunk(int frameSize,
                                             const NetworkFrame& partialFrame) {

        if (isFrameCompleted() ||
            masterChunkFound_ ||
            frameSize <= 0 ||
            frameSize < partialFrame.data.size() ||
            partialFrame.data.isEmpty())
            return false;

#if defined (NETWORK_PROTOCOL_EXTENDED) && (NETWORK_PROTOCOL_EXTENDED == 1)
        if (collectedChunks_ == 0) {
            frame_.id = partialFrame.id;
            frame_.number = partialFrame.number;
            frame_.interpretation = partialFrame.interpretation;
            frame_.time = partialFrame.time;
            frame_.priority = partialFrame.priority;
            frame_.task = partialFrame.task;
            frame_.flow = partialFrame.flow;

            if (frame_.data.size() < frameSize) frame_.data.resize(frameSize);
            frame_.data.replace(0, partialFrame.data.size(), partialFrame.data);

            collectedChunks_ = 1, detectedChunks_ = getChunkNumber(frameSize);
        }
        else {
            int detectedChunks = getChunkNumber(frameSize);

            if (detectedChunks < detectedChunks_ ||
                detectedChunks < collectedChunks_ + 1)
                return false;

            frame_.number = partialFrame.number;

            if (frame_.data.size() < frameSize) frame_.data.resize(frameSize);
            frame_.data.replace(0, partialFrame.data.size(), partialFrame.data);

            ++collectedChunks_;
            detectedChunks_ = detectedChunks;
        }
#else
        frame_.id = partialFrame.id;
        frame_.number = partialFrame.number;
        frame_.interpretation = partialFrame.interpretation;
        frame_.time = partialFrame.time;
        frame_.priority = partialFrame.priority;
        frame_.task = partialFrame.task;
        frame_.flow = partialFrame.flow;

        frame_.data.reserve(frameSize);
        frame_.data.append(partialFrame.data.data(), partialFrame.data.size());

        collectedChunks_ = 1, detectedChunks_ = getChunkNumber(frameSize);
#endif

        masterChunkFound_ = true;
        if (isFrameCompleted()) frame_.data.squeeze();

        return true;
    }

    /// Puts a slave chunk to the frame.
    /// \details Writes chunk data at the offset \a frameOffset to the frame
    /// buffer.
    /// \param[in]  frameOffset     Offset in frame data.
    /// \param[in]  partialFrame    Chunk data.
    /// \retval \c true on success.
    /// \retval \c false on error.
    bool NetworkFrameBuilder::putSlaveChunk(int frameOffset,
                                            const NetworkFrame& partialFrame) {

#if defined (NETWORK_PROTOCOL_EXTENDED) && (NETWORK_PROTOCOL_EXTENDED == 1)
        if (isFrameCompleted() ||
            frameOffset <= 0 ||
            partialFrame.data.isEmpty())
            return false;

        int frameSize = frameOffset + partialFrame.data.size();

        if (masterChunkFound_ && frameSize > frame_.data.size())
            return false;

        if (collectedChunks_ == 0) {
            frame_.id = partialFrame.id;
            frame_.number = partialFrame.number;
            frame_.interpretation = partialFrame.interpretation;
            frame_.time = partialFrame.time;
            frame_.priority = partialFrame.priority;
            frame_.task = partialFrame.task;
            frame_.flow = partialFrame.flow;
        }

        if (frame_.data.size() < frameSize) frame_.data.resize(frameSize);

        frame_.data.replace(frameOffset,
                            partialFrame.data.size(),
                            partialFrame.data);
#else
        Q_UNUSED(frameOffset)

        if (isFrameCompleted() ||
            !masterChunkFound_ ||
            partialFrame.data.isEmpty() ||
            frame_.data.capacity() <
                (frame_.data.size() + partialFrame.data.size()))
            return false;

        frame_.data.append(partialFrame.data.data(), partialFrame.data.size());
#endif

        ++collectedChunks_;
        if (isFrameCompleted()) frame_.data.squeeze();

        return true;
    }

    /// Calculates the number of chunks by frame size \a frameSize.
    /// \details Calculates the number of chunks, simulating a breakdown
    /// of the frame into datagrams.
    /// \param[in]  frameSize   Frame size.
    /// \return Number of chunks.
    int NetworkFrameBuilder::getChunkNumber(int frameSize) {
        auto result = 0;

        while (frameSize > 0) {
            auto datagramSize = DATAGRAM_DATA_MAX_SIZE;

            while (frameSize > 0 && datagramSize > 0) {
                auto headerSize = result == 0 ? CHUNK_MASTER_HEADER_SIZE
                                              : CHUNK_SLAVE_HEADER_SIZE;

                auto dataSize = result == 0 ? CHUNK_MASTER_DATA_MAX_SIZE
                                            : CHUNK_SLAVE_DATA_MAX_SIZE;

                if (datagramSize <= headerSize) break;

                datagramSize -= headerSize;
                dataSize = qMin(dataSize, qMin(datagramSize, frameSize));
                ++result, frameSize -= dataSize, datagramSize -= dataSize;
            }
        }

        return result;
    }

    /// Constructs a network serializer.
    /// \details Constructs a default network serializer.
    NetworkSerializer::NetworkSerializer()
        : NetworkSerializer(MemorySerializer::Endianness::BigEndian) {
    }

    /// Constructs a network serializer.
    /// \details Constructs a network serializer with the specified data endianness.
    /// \param[in]  endianness   Data endianness.
    NetworkSerializer::NetworkSerializer(MemorySerializer::Endianness endianness)
        : endianness_(endianness) {
    }

    /// Returns the data endianness.
    /// \details Returns the data endianness of either big-endian or little-endian.
    /// \return Data endianness.
    MemorySerializer::Endianness NetworkSerializer::endianness() const {
        return endianness_;
    }

    /// Serializes the network frame into datagrams.
    /// \details Serializes frame data and metadata into a list of datagrams.
    /// \param[in]  frame   Network frame.
    /// \return List of datagrams.
    std::list<QByteArray> NetworkSerializer::serialize(
        const NetworkFrame& frame) const {

        std::list<QByteArray> datagrams;
        serialize(frame, datagrams);
        return datagrams;
    }

    /// Serializes the network frame into datagrams.
    /// \details Serializes frame data and metadata into a list of datagrams.
    /// \param[in]  frame       Network frame.
    /// \param[out] datagrams   List of datagrams.
    void NetworkSerializer::serialize(const NetworkFrame& frame,
                                      std::list<QByteArray>& datagrams) const {

        if (frame.task.isEmpty() ||
            frame.flow.isEmpty() ||
            frame.data.isEmpty() ||
            frame.task.toUtf8().size() > CHUNK_TASK_SIZE ||
            frame.flow.toUtf8().size() > CHUNK_FLOW_SIZE ||
            frame.data.size() > FRAME_MAX_SIZE)
            return;

        auto index = 0, slaveChunkNumber = 1, frameSize = frame.data.size();
        auto taskArray = frame.task.toUtf8(), flowArray = frame.flow.toUtf8();

        taskArray.append(CHUNK_TASK_SIZE - taskArray.size(), '\0');
        flowArray.append(CHUNK_FLOW_SIZE - flowArray.size(), '\0');

        while (index < frameSize) {
            int left = frameSize - index, grow = 0, size = DATAGRAM_HEADER_SIZE;

            if (index == 0) {
                grow += qMin(left, CHUNK_MASTER_DATA_MAX_SIZE);
                size += CHUNK_MASTER_HEADER_SIZE + grow;
            }

            while (grow < left &&
                   DATAGRAM_MAX_SIZE - size > CHUNK_SLAVE_HEADER_SIZE) {

                auto freeSize =
                    DATAGRAM_MAX_SIZE - CHUNK_SLAVE_HEADER_SIZE - size;

                auto dataSize = qMin(freeSize, CHUNK_SLAVE_DATA_MAX_SIZE);
                auto packSize = qMin(dataSize, left - grow);
                auto allSize = CHUNK_SLAVE_HEADER_SIZE + packSize;

                size += allSize, grow += packSize;
            }

            QByteArray datagram;
            datagram.reserve(size);

            MemorySerializer serializer(&datagram, QIODevice::WriteOnly);
            serializer.setEndianness(endianness_);

            serializer << static_cast<quint16>(DATAGRAM_PROTOCOL_VERSION);
            serializer << static_cast<quint16>(datagram.capacity());
            serializer << static_cast<quint32>(0);
            serializer << static_cast<quint16>(0);

            while (datagram.size() < datagram.capacity()) {
                if (index == 0) {
                    auto freeSize = datagram.capacity() -
                                    datagram.size() -
                                    CHUNK_MASTER_HEADER_SIZE;

                    auto dataSize = qMin(freeSize, CHUNK_MASTER_DATA_MAX_SIZE);
                    auto allSize = CHUNK_MASTER_HEADER_SIZE + dataSize;

                    serializer << static_cast<quint8>(CHUNK_MASTER_ID);
                    serializer << static_cast<quint16>(allSize);
                    serializer.writeRawData(taskArray.data(), CHUNK_TASK_SIZE);
                    serializer.writeRawData(flowArray.data(), CHUNK_FLOW_SIZE);
                    serializer << static_cast<quint32>(frame.id);
                    serializer << static_cast<quint8>(frame.interpretation);
                    serializer << static_cast<quint8>(frame.priority);
                    serializer << static_cast<quint16>(frame.time);
                    serializer << static_cast<quint16>(frame.number);
                    serializer << static_cast<quint32>(frameSize);
                    serializer.writeRawData(frame.data.data() + index, dataSize);

                    index += dataSize;

                } else {
                    auto freeSize = datagram.capacity() -
                                    datagram.size() -
                                    CHUNK_SLAVE_HEADER_SIZE;

                    auto dataSize = qMin(freeSize, CHUNK_SLAVE_DATA_MAX_SIZE);
                    auto allSize = CHUNK_SLAVE_HEADER_SIZE + dataSize;

                    serializer << static_cast<quint8>(CHUNK_SLAVE_ID);
                    serializer << static_cast<quint16>(allSize);
                    serializer.writeRawData(taskArray.data(), CHUNK_TASK_SIZE);
                    serializer.writeRawData(flowArray.data(), CHUNK_FLOW_SIZE);
                    serializer << static_cast<quint32>(frame.id);
                    serializer << static_cast<quint8>(frame.interpretation);
                    serializer << static_cast<quint8>(frame.priority);
                    serializer << static_cast<quint16>(frame.time);
                    serializer << static_cast<quint16>(slaveChunkNumber++);
#if defined (NETWORK_PROTOCOL_EXTENDED) && (NETWORK_PROTOCOL_EXTENDED == 1)
                    serializer << static_cast<quint32>(index);
#endif
                    serializer.writeRawData(frame.data.data() + index, dataSize);

                    index += dataSize;
                }
            }

            serializer.seek(8);
            serializer << Utility::crc16(datagram);

            if (serializer.status() != MemorySerializer::Status::Ok) {
                datagrams.clear();
                break;
            }
            else datagrams.push_back(datagram);
        }
    }

    /// Deserializes a datagram to collect frames.
    /// \details Deserializes a datagram to collect frames and other messages.
    /// \param[in]  data    Datagram data to parse.
    /// \param[in]  size    Datagram data size.
    void NetworkSerializer::deserialize(const char* data, int size) {
        deserialize(QByteArray::fromRawData(data, size));
    }

    /// Deserializes a datagram to collect frames.
    /// \details Deserializes a datagram to collect frames and other messages.
    /// \param[in]  datagram    Datagram to parse.
    void NetworkSerializer::deserialize(const QByteArray& datagram) {
        if (datagram.size() <= DATAGRAM_HEADER_SIZE ||
            datagram.size() > DATAGRAM_MAX_SIZE) return;

        MemorySerializer serializer(datagram);
        serializer.setEndianness(endianness_);

        quint16 datagramVersion;
        serializer >> datagramVersion;

        quint16 datagramSize;
        serializer >> datagramSize;

        quint32 datagramRTC;
        serializer >> datagramRTC;

        quint16 datagramCRC16;
        serializer >> datagramCRC16;

        if (datagramVersion != DATAGRAM_PROTOCOL_VERSION ||
            datagramSize != datagram.size() ||
            datagramCRC16 != Utility::crc16(datagram, {8, 9}))
            return;

        while (serializer.bytesAvailable() >
               qMin(CHUNK_MASTER_HEADER_SIZE, CHUNK_SLAVE_HEADER_SIZE)) {

            quint8 chunkID;
            serializer >> chunkID;

            if (chunkID == CHUNK_MASTER_ID) {
                if (serializer.bytesAvailable() < CHUNK_MASTER_HEADER_SIZE)
                    break;

                quint16 chunkSize;
                serializer >> chunkSize;

                QByteArray frameTask(CHUNK_TASK_SIZE, Qt::Uninitialized);
                serializer.readRawData(frameTask.data(), frameTask.size());

                QByteArray frameFlow(CHUNK_FLOW_SIZE, Qt::Uninitialized);
                serializer.readRawData(frameFlow.data(), frameFlow.size());

                quint32 frameID;
                serializer >> frameID;

                quint8 frameInterpretation;
                serializer >> frameInterpretation;

                quint8 framePriority;
                serializer >> framePriority;

                quint16 frameTime;
                serializer >> frameTime;

                quint16 frameNumber;
                serializer >> frameNumber;

                quint32 frameSize;
                serializer >> frameSize;

                if (chunkSize <= CHUNK_MASTER_HEADER_SIZE ||
                    chunkSize > CHUNK_MAX_SIZE ||
                    frameSize > FRAME_MAX_SIZE ||
                    (chunkSize - CHUNK_MASTER_HEADER_SIZE) >
                        serializer.bytesAvailable())
                    break;

                auto data = datagram.data() + serializer.position();
                auto size = chunkSize - CHUNK_MASTER_HEADER_SIZE;

                serializer.skipRawData(size);

                NetworkFrame frame;
                frame.id = frameID;
                frame.number = frameNumber;
                frame.interpretation = frameInterpretation;
                frame.time = frameTime;
                frame.priority = framePriority;
                frame.task = frameTask;
                frame.flow = frameFlow;
                frame.data = QByteArray::fromRawData(data, size);

                auto iterator = collectedFrames_.find(frameID);

                if (iterator == collectedFrames_.end())
                    iterator = collectedFrames_.insert(frameID,
                                                       NetworkFrameBuilder{ });

                iterator.value().putMasterChunk(frameSize, frame);
            }
            else if (chunkID == CHUNK_SLAVE_ID) {
                if (serializer.bytesAvailable() < CHUNK_SLAVE_HEADER_SIZE)
                    break;

                quint16 chunkSize;
                serializer >> chunkSize;

                QByteArray frameTask(CHUNK_TASK_SIZE, Qt::Uninitialized);
                serializer.readRawData(frameTask.data(), frameTask.size());

                QByteArray frameFlow(CHUNK_FLOW_SIZE, Qt::Uninitialized);
                serializer.readRawData(frameFlow.data(), frameFlow.size());

                quint32 frameID;
                serializer >> frameID;

                quint8 frameInterpretation;
                serializer >> frameInterpretation;

                quint8 framePriority;
                serializer >> framePriority;

                quint16 frameTime;
                serializer >> frameTime;

                quint16 slaveChunkNumber;
                serializer >> slaveChunkNumber;

#if defined (NETWORK_PROTOCOL_EXTENDED) && (NETWORK_PROTOCOL_EXTENDED == 1)
                quint32 frameOffset;
                serializer >> frameOffset;
#endif

                if (chunkSize <= CHUNK_SLAVE_HEADER_SIZE ||
                    chunkSize > CHUNK_MAX_SIZE ||
                    (chunkSize - CHUNK_SLAVE_HEADER_SIZE) >
                        serializer.bytesAvailable())
                    break;

                auto data = datagram.data() + serializer.position();
                auto size = chunkSize - CHUNK_SLAVE_HEADER_SIZE;

                serializer.skipRawData(size);

                NetworkFrame frame;
                frame.id = frameID;
                frame.interpretation = frameInterpretation;
                frame.time = frameTime;
                frame.priority = framePriority;
                frame.task = frameTask;
                frame.flow = frameFlow;
                frame.data = QByteArray::fromRawData(data, size);

                auto iterator = collectedFrames_.find(frameID);

#if defined (NETWORK_PROTOCOL_EXTENDED) && (NETWORK_PROTOCOL_EXTENDED == 1)
                if (iterator == collectedFrames_.end())
                    iterator = collectedFrames_.insert(frameID,
                                                       NetworkFrameBuilder{ });

                iterator.value().putSlaveChunk(frameOffset, frame);
#else
                if (iterator != collectedFrames_.end())
                    iterator.value().putSlaveChunk(0, frame);
#endif
            }
            else break;
        }
    }

    /// Returns completed frames.
    /// \details Returns frames that received all their data.
    /// \return List of frames.
    std::list<NetworkFrame> NetworkSerializer::completedFrames() {
        std::list<NetworkFrame> frames;
        completedFrames(frames);
        return frames;
    }

    /// Returns completed frames.
    /// \details Returns frames that received all their data.
    /// \param[out]	frames	List of frames.
    void NetworkSerializer::completedFrames(std::list<NetworkFrame>& frames) {
        auto iterator = collectedFrames_.begin();
        while (iterator != collectedFrames_.end()) {
            if (iterator.value().isFrameCompleted()) {
                frames.push_back(iterator.value().getFrame());
                iterator = collectedFrames_.erase(iterator);
            }
            else ++iterator;
        }
    }

    /// Clears pending frames.
    /// \details Clears all completed and uncompleted frames.
    void NetworkSerializer::clear() {
        collectedFrames_.clear();
    }
}
