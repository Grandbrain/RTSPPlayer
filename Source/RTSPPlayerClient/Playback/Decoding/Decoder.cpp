/// \file Decoder.cpp
/// \brief Contains classes and functions definitions that provide audio and
/// video decoder implementation.
/// \bug No known bugs.

#include "AudioDecoder.hpp"
#include "VideoDecoder.hpp"

extern "C" {
	#include <libavcodec/avcodec.h>
	#include <libavutil/imgutils.h>
	#include <libswscale/swscale.h>
	#include <libswresample/swresample.h>
}

namespace {

	///
	/// \details
	enum class DecoderStatusCode {

		///
		Error,

		///
		FrameReceived,

		///
		NeedMoreData,

		///
		ReceiveFrameFirst,

		///
		DecoderFlushed
	};

	///
	/// \details
	struct DecoderContext final {

		///
		/// \details
		AVCodec* codec = nullptr;

		///
		/// \details
		AVCodecContext* codecContext = nullptr;

		///
		/// \details
		AVFrame* frame = nullptr;

		///
		/// \details
		AVPacket* packet = nullptr;
	};

	///
	/// \details
	struct ScalerContext final {

		///
		/// \details
		int inWidth;

		///
		/// \details
		int inHeight;

		///
		/// \details
		AVPixelFormat inFormat;

		///
		/// \details
		int outWidth;

		///
		/// \details
		int outHeight;

		///
		/// \details
		AVPixelFormat outFormat;

		///
		/// \details
		AVPixelFormat modifiedFormat;

		///
		/// \details
		int flags;

		///
		/// \details
		AVFrame* frame = nullptr;

		///
		/// \details
		SwsContext* scalerContext = nullptr;
	};

	///
	/// \details
	struct ResamplerContext final {

		///
		/// \details
		int inChannels;

		///
		/// \details
		int inSampleRate;

		///
		/// \details
		uint64_t inChannelLayout;

		///
		/// \details
		AVSampleFormat inSampleFormat;

		///
		/// \details
		int outChannels;

		///
		/// \details
		int outSampleRate;

		///
		/// \details
		uint64_t outChannelLayout;

		///
		/// \details
		AVSampleFormat outSampleFormat;

		///
		/// \details
		AVFrame* frame = nullptr;

		///
		/// \details
		SwrContext* resamplerContext = nullptr;
	};

	///
	/// \details
	/// \param[in]	format
	/// \return
	inline auto adjustFormat(AVPixelFormat format) noexcept {
		switch (format) {
		case AV_PIX_FMT_YUVJ420P:
			format = AV_PIX_FMT_YUV420P;
			break;
		case AV_PIX_FMT_YUVJ422P:
			format = AV_PIX_FMT_YUV422P;
			break;
		case AV_PIX_FMT_YUVJ444P:
			format = AV_PIX_FMT_YUV444P;
			break;
		case AV_PIX_FMT_YUVJ440P:
			format = AV_PIX_FMT_YUV440P;
			break;
		default:
			break;
		}

		return format;
	}

	///
	/// \details
	/// \param[in]	format
	/// \return
	inline auto convertFormat(AVPixelFormat format) noexcept {
		auto result = QImage::Format_Invalid;
		format = adjustFormat(format);

		switch (format) {
		case AV_PIX_FMT_MONOBLACK:
		case AV_PIX_FMT_MONOWHITE:
			result = QImage::Format_Mono;
			break;
		case AV_PIX_FMT_GRAY8:
			result = QImage::Format_Grayscale8;
			break;
		case AV_PIX_FMT_GRAY16:
			result = QImage::Format_Grayscale16;
			break;
		case AV_PIX_FMT_RGB24:
			result = QImage::Format_RGB888;
			break;
		default:
			break;
		}

		return result;
	}

	///
	/// \details
	/// \param[in]	format
	/// \return
	inline auto convertFormat(Decoders::VideoDecoder::Format format) noexcept {
		auto result = AV_PIX_FMT_NONE;

		switch (format) {
		case Decoders::VideoDecoder::Format::Mono:
			result = AV_PIX_FMT_MONOBLACK;
			break;
		case Decoders::VideoDecoder::Format::Grayscale8:
			result = AV_PIX_FMT_GRAY8;
			break;
		case Decoders::VideoDecoder::Format::Grayscale16:
			result = AV_PIX_FMT_GRAY16;
			break;
		case Decoders::VideoDecoder::Format::RGB888:
			result = AV_PIX_FMT_RGB24;
			break;
		default:
			break;
		}

		return result;
	}

	///
	/// \details
	/// \param[in]	codec
	/// \return
	inline auto convertCodec(Decoders::VideoDecoder::Codec codec) noexcept {
		auto result = AV_CODEC_ID_NONE;

		switch (codec) {
		case Decoders::VideoDecoder::Codec::H264:
			result = AV_CODEC_ID_H264;
			break;
		case Decoders::VideoDecoder::Codec::MJPEG:
			result = AV_CODEC_ID_MJPEG;
			break;
		default:
			break;
		}

		return result;
	}

	///
	/// \details
	/// \param[in,out]	decoderContext
	auto destroy(DecoderContext& decoderContext) noexcept {
		av_packet_free(&decoderContext.packet);
		av_frame_free(&decoderContext.frame);
		avcodec_free_context(&decoderContext.codecContext);

		decoderContext.packet = nullptr;
		decoderContext.frame = nullptr;
		decoderContext.codecContext = nullptr;
		decoderContext.codec = nullptr;
	}

	///
	/// \details
	/// \param[in,out]	scalerContext
	auto destroy(ScalerContext& scalerContext) noexcept {
		if (scalerContext.frame) {
			av_freep(&scalerContext.frame->data[0]);
		}

		av_frame_free(&scalerContext.frame);
		sws_freeContext(scalerContext.scalerContext);

		scalerContext.frame = nullptr;
		scalerContext.scalerContext = nullptr;
	}

	///
	/// \details
	/// \param[in,out]	resamplerContext
	auto destroy(ResamplerContext& resamplerContext) noexcept {
		av_frame_free(&resamplerContext.frame);
		swr_free(&resamplerContext.resamplerContext);

		resamplerContext.frame = nullptr;
		resamplerContext.resamplerContext = nullptr;
	}

	///
	/// \details
	/// \param[in]		codecID
	/// \param[in,out]	decoderContext
	/// \param[in]		bitsPerCodedSample
	/// \retval
	/// \retval
	auto initialize(AVCodecID codecID,
					DecoderContext& decoderContext,
					int bitsPerCodedSample = 0) noexcept {

		destroy(decoderContext);

		decoderContext.codec = avcodec_find_decoder(codecID);

		if (!decoderContext.codec) {
			destroy(decoderContext);
			return false;
		}

		decoderContext.codecContext =
			avcodec_alloc_context3(decoderContext.codec);

		if (!decoderContext.codecContext) {
			destroy(decoderContext);
			return false;
		}

		if (avcodec_get_type(codecID) == AVMEDIA_TYPE_AUDIO) {
			if (codecID == AV_CODEC_ID_PCM_MULAW ||
				codecID == AV_CODEC_ID_PCM_ALAW) {
				decoderContext.codecContext->sample_rate = 8000;
				decoderContext.codecContext->channels = 1;
			}

			decoderContext.codecContext->bits_per_coded_sample =
				bitsPerCodedSample;
		}

		if (avcodec_open2(decoderContext.codecContext,
						  decoderContext.codec,
						  nullptr) < 0) {

			destroy(decoderContext);
			return false;
		}

		decoderContext.frame = av_frame_alloc();

		if (!decoderContext.frame) {
			destroy(decoderContext);
			return false;
		}

		decoderContext.packet = av_packet_alloc();

		if (!decoderContext.packet) {
			destroy(decoderContext);
			return false;
		}

		return true;
	}

	///
	/// \details
	/// \param[in,out]	scalerContext
	/// \retval
	/// \retval
	auto initialize(ScalerContext& scalerContext) noexcept {
		destroy(scalerContext);

		scalerContext.scalerContext =
			sws_getContext(scalerContext.inWidth,
						   scalerContext.inHeight,
						   scalerContext.inFormat,
						   scalerContext.outWidth,
						   scalerContext.outHeight,
						   scalerContext.outFormat,
						   scalerContext.flags,
						   nullptr,
						   nullptr,
						   nullptr);

		if (!scalerContext.scalerContext) {
			destroy(scalerContext);
			return false;
		}

		scalerContext.frame = av_frame_alloc();

		if (!scalerContext.frame) {
			destroy(scalerContext);
			return false;
		}

		if (av_image_alloc(scalerContext.frame->data,
						   scalerContext.frame->linesize,
						   scalerContext.outWidth,
						   scalerContext.outHeight,
						   scalerContext.outFormat,
						   16) < 0) {
			destroy(scalerContext);
			return false;
		}

		return true;
	}

	///
	/// \details
	/// \param[in,out]	resamplerContext
	/// \retval
	/// \retval
	auto initialize(ResamplerContext& resamplerContext) noexcept {
		destroy(resamplerContext);

		resamplerContext.frame = av_frame_alloc();

		if (!resamplerContext.frame) {
			destroy(resamplerContext);
			return false;
		}

		resamplerContext.resamplerContext =
			swr_alloc_set_opts(nullptr,
							   resamplerContext.outChannelLayout,
							   resamplerContext.outSampleFormat,
							   resamplerContext.outSampleRate,
							   resamplerContext.inChannelLayout,
							   resamplerContext.inSampleFormat,
							   resamplerContext.inSampleRate,
							   0,
							   nullptr);

		if (!resamplerContext.resamplerContext ||
			swr_init(resamplerContext.resamplerContext) < 0) {
			destroy(resamplerContext);
			return false;
		}

		return true;
	}

	///
	/// \details
	/// \param[in]		data
	/// \param[in]		size
	/// \param[in,out]	packet
	/// \retval
	/// \retval
	auto setData(const char* data, int size, AVPacket* packet) noexcept {
		if (!packet) return false;

		if (!data || size <= 0) {
			av_packet_unref(packet);
			return true;
		}

		if (size < packet->size) {
			av_shrink_packet(packet, size);
		}
		else if (size > packet->size) {
			int result = av_grow_packet(packet, size - packet->size);
			if (result < 0) {
				av_packet_unref(packet);
				return false;
			}
		}

		memcpy(packet->data, data, size);

		return true;
	}

	///
	/// \details
	/// \param[in]		data
	/// \param[in]		size
	/// \param[in,out]	decoderContext
	/// \retval
	/// \retval
	auto setExtradata(const char* data,
					  int size,
					  DecoderContext& decoderContext) noexcept {

		if (!decoderContext.codecContext)
			return false;

		if (!data || size <= 0) {
			av_free(decoderContext.codecContext->extradata);

			decoderContext.codecContext->extradata = nullptr;
			decoderContext.codecContext->extradata_size = 0;

			return true;
		}

		if (!decoderContext.codecContext->extradata ||
			decoderContext.codecContext->extradata_size < size) {

			av_free(decoderContext.codecContext->extradata);

			decoderContext.codecContext->extradata = static_cast<uint8_t*>(
				av_malloc(size + AV_INPUT_BUFFER_PADDING_SIZE)
			);

			if (!decoderContext.codecContext->extradata) {
				decoderContext.codecContext->extradata = nullptr;
				decoderContext.codecContext->extradata_size = 0;
				return false;
			}
		}

		decoderContext.codecContext->extradata_size = size;

		memcpy(decoderContext.codecContext->extradata, data, size);

		memset(decoderContext.codecContext->extradata + size,
			   0,
			   AV_INPUT_BUFFER_PADDING_SIZE);

		avcodec_close(decoderContext.codecContext);

		if (avcodec_open2(decoderContext.codecContext,
						  decoderContext.codec,
						  nullptr) < 0)
			return false;

		return true;
	}

	///
	/// \details
	/// \param[in]		codecContext
	/// \param[in,out]	frame
	/// \param[in]		packet
	/// \return
	auto decode(AVCodecContext* codecContext,
				AVFrame* frame,
				AVPacket* packet = nullptr) noexcept {

		if (!codecContext || !frame)
			return DecoderStatusCode::Error;

		if (packet) {
			int result = avcodec_send_packet(codecContext, packet);
			if (result < 0) {
				if (result == AVERROR(EAGAIN))
					return DecoderStatusCode::ReceiveFrameFirst;
				else if (result == AVERROR_EOF)
					return DecoderStatusCode::DecoderFlushed;
				else
					return DecoderStatusCode::Error;
			}
		}

		int result = avcodec_receive_frame(codecContext, frame);
		if (result < 0) {
			if (result == AVERROR(EAGAIN))
				return DecoderStatusCode::NeedMoreData;
			else if (result == AVERROR_EOF)
				return DecoderStatusCode::DecoderFlushed;
			else
				return DecoderStatusCode::Error;
		}

		return DecoderStatusCode::FrameReceived;
	}

	///
	/// \details
	/// \param[in]		inputFrame
	/// \param[in,out]	outputFrame
	/// \param[in]		scalerContext
	/// \retval
	/// \retval
	auto scale(const AVFrame* inputFrame,
			   AVFrame* outputFrame,
			   SwsContext* scalerContext) noexcept {

		if (!inputFrame || ! outputFrame || !scalerContext)
			return false;

		sws_scale(scalerContext,
				  inputFrame->data,
				  inputFrame->linesize,
				  0,
				  inputFrame->height,
				  outputFrame->data,
				  outputFrame->linesize);

		return true;
	}
}

/// A namespace that contains classes and functions for decoding media.
namespace Decoders {

	///
	/// \details
	class VideoDecoder::VideoDecoderPrivate final {
	public:

		///
		/// \details
		VideoDecoderPrivate() noexcept {

		}

		///
		/// \details
		virtual ~VideoDecoderPrivate() noexcept {
			destroy();
		}

	public:

		///
		/// \details
		/// \param[in]	codecID
		/// \param[in]	format
		/// \retval
		/// \retval
		bool initialize(AVCodecID codecID, AVPixelFormat format) noexcept {
			return setFormat(format) && ::initialize(codecID, decoderContext_);
		}

		///
		/// \details
		void destroy() noexcept {
			::destroy(scalerContext_);
			::destroy(decoderContext_);
		}

		///
		/// \details
		/// \return
		const QImage& getFrame() const noexcept {
			return lastFrame_;
		}

		///
		/// \details
		/// \return
		QImage& getFrame() noexcept {
			return lastFrame_;
		}

		///
		/// \details
		/// \param[in]	format
		/// \retval
		/// \retval
		bool setFormat(AVPixelFormat format) noexcept {
			if (format == AV_PIX_FMT_NONE) return false;

			scalerContext_.modifiedFormat = adjustFormat(format);

			return true;
		}

		///
		/// \details
		/// \param[in]	data
		/// \retval
		/// \retval
		bool setExtradata(const QByteArray& data) noexcept {
			return ::setExtradata(data.data(), data.size(), decoderContext_);
		}

		///
		/// \details
		/// \param[in]	data
		/// \retval
		/// \retval
		bool decode(const QByteArray& data) noexcept {
			if (!::setData(data.data(), data.size(), decoderContext_.packet))
				return false;

			auto statusCode = ::decode(decoderContext_.codecContext,
									   decoderContext_.frame,
									   decoderContext_.packet);

			if (statusCode == DecoderStatusCode::Error) {
				return false;
			}
			else if (statusCode == DecoderStatusCode::DecoderFlushed ||
					 statusCode == DecoderStatusCode::NeedMoreData) {
				return true;
			}

			while (statusCode == DecoderStatusCode::FrameReceived ||
				   statusCode == DecoderStatusCode::ReceiveFrameFirst) {

				if (statusCode == DecoderStatusCode::FrameReceived &&
					initializeScalerContext(decoderContext_.codecContext) &&
					::scale(decoderContext_.frame, scalerContext_.frame,
							scalerContext_.scalerContext)) {

					auto format = convertFormat(scalerContext_.outFormat);

					lastFrame_ = QImage(scalerContext_.frame->data[0],
										scalerContext_.outWidth,
										scalerContext_.outHeight,
                                        format);
				}

				statusCode = ::decode(decoderContext_.codecContext,
									  decoderContext_.frame);
			}

			return statusCode != DecoderStatusCode::Error;
		}

	private:

		///
		/// \details
		/// \retval
		/// \retval
		bool initializeScalerContext(AVCodecContext* codecContext) noexcept {
			if (!codecContext) return false;

			if (!scalerContext_.scalerContext ||
				scalerContext_.inWidth != codecContext->width ||
				scalerContext_.inHeight != codecContext->height ||
				scalerContext_.inFormat != adjustFormat(codecContext->pix_fmt)||
				scalerContext_.outFormat != scalerContext_.modifiedFormat) {

				scalerContext_.inWidth = codecContext->width;
				scalerContext_.inHeight = codecContext->height;
				scalerContext_.inFormat = adjustFormat(codecContext->pix_fmt);
				scalerContext_.outWidth = scalerContext_.inWidth;
				scalerContext_.outHeight = scalerContext_.inHeight;
				scalerContext_.outFormat = scalerContext_.modifiedFormat;
				scalerContext_.flags = SWS_BICUBIC;

				return ::initialize(scalerContext_);
			}

			return true;
		}

	private:

		///
		/// \details
		QImage lastFrame_;

		///
		/// \details
		DecoderContext decoderContext_;

		///
		/// \details
		ScalerContext scalerContext_;
	};

	///
	/// \details
	/// \param[in]	parent
	VideoDecoder::VideoDecoder(QObject* parent)
		: QObject(parent),
		  private_(new VideoDecoderPrivate()) {

	}

	///
	/// \details
	VideoDecoder::~VideoDecoder() {
		destroy();
	}

	///
	/// \details
	/// \param[in]	codec
	/// \param[in]	format
	/// \retval
	/// \retval
	bool VideoDecoder::initialize(Codec codec, Format format) {
		return private_->initialize(convertCodec(codec), convertFormat(format));
	}

	///
	/// \details
	void VideoDecoder::destroy() {
		private_->destroy();
	}

	///
	/// \details
	/// \param[in]	format
	void VideoDecoder::setFormat(Format format) {
		if (!private_->setFormat(convertFormat(format)))
			emit onError(Error::FormatError);
	}

	///
	/// \details
	/// \param[in]	data
	void VideoDecoder::setExtradata(const QByteArray& data) {
		if (!private_->setExtradata(data))
			emit onError(Error::ExtradataError);
	}

	///
	/// \details
	/// \param[in]	data
	void VideoDecoder::decode(const QByteArray& data) {
		if (private_->decode(data))
            emit onFrame(private_->getFrame().copy());
		else emit onError(Error::DecoderError);
	}
}
