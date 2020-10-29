/// \file InterprocessSerializer.cpp
/// \brief Contains definitions of classes and functions for processing
/// interprocess data.
/// \bug No known bugs.

#include "InterprocessSerializer.hpp"

namespace {

    /// Encodes a regular byte array to Base64 format.
    /// \details Encodes a byte array to Base64 format based on RFC 4648.
    /// \param[in]  array   Regular byte array.
    /// \return Encoded Base64 byte array.
    QByteArray encodeBase64(const QByteArray& array) {
        return array.toBase64(QByteArray::Base64Encoding |
                              QByteArray::OmitTrailingEquals);
    }

    /// Decodes a Base64 byte array to a regular byte array.
    /// \details Decodes a byte array from Base64 format based on RFC 4648.
    /// \param[in]  array   Base64 byte array.
    /// \return Decoded regular byte array.
    QByteArray decodeBase64(const QByteArray& array) {
        return QByteArray::fromBase64(array, QByteArray::Base64Encoding |
                                      QByteArray::IgnoreBase64DecodingErrors);
    }
}

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
            array
                    += encodeBase64(it.key().toUtf8())
                    + '='
                    + encodeBase64(it.value().toUtf8())
                    + ' ';

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

            auto key = decodeBase64(pair.left(index).trimmed());
            auto value = decodeBase64(pair.mid(index + 1).trimmed());

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
