/// \file InterprocessSerializer.cpp
/// \brief Contains definitions of classes and functions for processing
/// interprocess data.
/// \bug No known bugs.

#include "InterprocessSerializer.hpp"

/// A namespace that contains common classes and functions for data
/// serialization.
namespace Common::Serialization {

    /// Serializes the interprocess frame into a byte array.
    /// \details Delegates serialization to an overloaded function.
    /// \param[in]  frame   Interprocess frame.
    /// \return Byte array.
    QByteArray InterprocessSerializer::serialize(
        const InterprocessFrame& frame) const {

        QByteArray array;
        serialize(frame, array);
        return array;
    }

    /// Serializes the interprocess frame into a byte array.
    /// \details Concatenates the key-value dictionary into a byte array.
    /// \param[in]  frame   Interprocess frame.
    /// \param[out] array   Byte array.
    void InterprocessSerializer::serialize(const InterprocessFrame& frame,
                                           QByteArray& array) const {

        auto begin = frame.parameterDictionary.cbegin();
        auto end = frame.parameterDictionary.cend();

        for (auto it = begin; it != end; ++it)
            array += it.key().toUtf8() + '=' + it.value().toUtf8() + ' ';

        array += '\n';
    }

    /// Deserializes the byte array into an interprocess frame.
    /// \details Delegates deserialization to an overloaded function.
    /// \param[in]  array   Byte array.
    /// \return Interprocess frame.
    InterprocessFrame InterprocessSerializer::deserialize(
        const QByteArray& array) {

        InterprocessFrame frame;
        deserialize(array, frame);
        return frame;
    }

    /// Deserializes the data buffer into an interprocess frame.
    /// \details Delegates deserialization to an overloaded function.
    /// \param[in]  data    Data buffer.
    /// \param[in]  size    Buffer size.
    /// \return Interprocess frame.
    InterprocessFrame InterprocessSerializer::deserialize(
        const char* data, int size) {

        InterprocessFrame frame;
        deserialize(data, size, frame);
        return frame;
    }

    /// Deserializes the byte array into an interprocess frame.
    /// \details Parses the byte array into a key-value dictionary.
    /// \param[in]  array   Byte array.
    /// \param[out] frame   Interprocess frame.
    void InterprocessSerializer::deserialize(const QByteArray& array,
                                             InterprocessFrame& frame) {

        auto pairs = array.toLower().split(' ');

        for (const auto& pair : pairs) {
            auto index = pair.indexOf('=');
            if (index <= 0 || index >= pair.size() - 1) continue;

            auto key = pair.left(index).trimmed();
            auto value = pair.mid(index + 1).trimmed();

            if (!key.isEmpty())
                frame.parameterDictionary.insert(key, value);
        }
    }

    /// Deserializes the data buffer into an interprocess frame.
    /// \details Delegates deserialization to an overloaded function.
    /// \param[in]  data    Data buffer.
    /// \param[in]  size    Buffer size.
    /// \param[out] frame   Interprocess frame.
    void InterprocessSerializer::deserialize(const char* data, int size,
                                             InterprocessFrame& frame) {

        deserialize(QByteArray::fromRawData(data, size), frame);
    }
}
