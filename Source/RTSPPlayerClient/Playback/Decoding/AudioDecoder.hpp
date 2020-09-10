/// \file AudioDecoder.hpp
/// \brief Contains classes and functions declarations that provide audio
/// decoder implementation.
/// \bug No known bugs.

#ifndef AUDIODECODER_HPP
#define AUDIODECODER_HPP

#include <QObject>

/// A namespace that contains classes and functions for decoding media.
namespace Decoders {

	///
	class AudioDecoder : public QObject {

		Q_OBJECT

	public:

		///
		/// \param[in]	parent
		explicit AudioDecoder(QObject* parent = nullptr) { }

		///
		~AudioDecoder() override { }

	public:

	public slots:

	signals:

	private:

	};
}

#endif
