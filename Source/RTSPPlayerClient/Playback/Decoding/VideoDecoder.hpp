/// \file VideoDecoder.hpp
/// \brief Contains classes and functions declarations that provide video
/// decoder implementation.
/// \bug No known bugs.

#ifndef VIDEODECODER_HPP
#define VIDEODECODER_HPP

#include <QImage>
#include <QByteArray>
#include <QLinkedList>
#include <QScopedPointer>

/// A namespace that contains classes and functions for decoding media.
namespace Decoders {

	///
	class VideoDecoder : public QObject {

		Q_OBJECT

	public:

		///
		enum class Codec {
			H264			,	///<
			MJPEG			,	///<
		};

		///
		enum class Format {
			Mono			,	///<
			Grayscale8		,	///<
			Grayscale16		,	///<
			RGB888			,	///<
		};

		///
		enum class Error {
			FormatError		,	///<
			ExtradataError	,	///<
			DecoderError	,	///<
		};

	public:

		///
		/// \param[in]	parent
		explicit VideoDecoder(QObject* parent = nullptr);

		///
		~VideoDecoder() override;

	public:

		///
		/// \param[in]	codec
		/// \param[in]	format
		/// \retval
		/// \retval
		bool initialize(Codec codec, Format format);

		///
		void destroy();

	public slots:

		///
		/// \param[in]	format
		void setFormat(Format format);

		///
		/// \param[in]	data
		void setExtradata(const QByteArray& data);

		///
		/// \param[in]	data
		void decode(const QByteArray& data);

	signals:

		///
		/// \param[in]	error
		void onError(Error error);

		///
		/// \param[in]	frame
		void onFrame(const QImage& frame);

	private:

		///
		class VideoDecoderPrivate;

		///
		QScopedPointer<VideoDecoderPrivate> private_;
	};
}

#endif
