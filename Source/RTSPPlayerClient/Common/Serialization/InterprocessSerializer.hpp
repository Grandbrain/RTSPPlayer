/// \file InterprocessSerializer.hpp
/// \brief Contains declarations of classes and functions for processing
/// interprocess data.
/// \bug No known bugs.

#ifndef INTERPROCESSSERIALIZER_HPP
#define INTERPROCESSSERIALIZER_HPP

#include <QHash>
#include <QByteArray>

/// A namespace that contains common classes and functions for data
/// serialization.
namespace Common::Serialization {

    /// A structure that defines an interprocess frame.
    struct InterprocessFrame {

        /// Dictionary of string parameters.
        QHash<QString, QString> parameterDictionary;
    };

    /// A class that provides an interprocess serializer implementation.
    class InterprocessSerializer {
    public:

        /// Constructs an interprocess serializer.
        explicit InterprocessSerializer() = default;

        /// Destroys the interprocess serializer.
        virtual ~InterprocessSerializer() = default;

    public:

        /// Serializes the interprocess frame into a byte array.
        /// \param[in]  frame   Interprocess frame.
        /// \return Byte array.
        QByteArray serialize(const InterprocessFrame& frame) const;

        /// Serializes the interprocess frame into a byte array.
        /// \param[in]  frame   Interprocess frame.
        /// \param[out] array   Byte array.
        void serialize(const InterprocessFrame& frame, QByteArray& array) const;

        /// Deserializes the byte array into an interprocess frame.
        /// \param[in]  array   Byte array.
        /// \return Interprocess frame.
        InterprocessFrame deserialize(const QByteArray& array);

        /// Deserializes the data buffer into an interprocess frame.
        /// \param[in]  data    Data buffer.
        /// \param[in]  size    Buffer size.
        /// \return Interprocess frame.
        InterprocessFrame deserialize(const char* data, int size);

        /// Deserializes the byte array into an interprocess frame.
        /// \param[in]  array   Byte array.
        /// \param[out] frame   Interprocess frame.
        void deserialize(const QByteArray& array, InterprocessFrame& frame);

        /// Deserializes the data buffer into an interprocess frame.
        /// \param[in]  data    Data buffer.
        /// \param[in]  size    Buffer size.
        /// \param[out] frame   Interprocess frame.
        void deserialize(const char* data, int size, InterprocessFrame& frame);
    };
}

#endif
