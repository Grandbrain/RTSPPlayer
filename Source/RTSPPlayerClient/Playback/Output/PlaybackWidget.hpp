/// \file PlaybackWidget.hpp
/// \brief Contains classes and functions declarations that provide playback
/// widget implementation.
/// \bug No known bugs.

#ifndef PLAYBACKWIDGET_HPP
#define PLAYBACKWIDGET_HPP

#include <QOpenGLBuffer>
#include <QOpenGLTexture>
#include <QOpenGLWidget>
#include <QOpenGLShaderProgram>
#include <QOpenGLFunctions>

///
namespace Player {

	///
	namespace Playback {

		///
		class PlaybackWidget
			: public QOpenGLWidget,
			  protected QOpenGLFunctions {

			Q_OBJECT

		public:

			/// Constructor.
			/// \param[in]	parent	Parent object.
			explicit PlaybackWidget(QWidget* parent = nullptr);

			/// Destructor.
			virtual ~PlaybackWidget();

		public:

			void setImage(const QImage& image);


		protected:

			///
			void initializeGL() override;

			///
			void paintGL() override;

			///
			/// \param[in]	width
			/// \param[in]	height
			void resizeGL(int width, int height) override;

		private:

			///
			void initializeResources();

			///
			void destroyResources();

		private:

			///
			QColor clearColor_;

			///
			QOpenGLBuffer vertexBuffer_;

			///
			QOpenGLTexture colorTexture_;

			///
			QOpenGLShaderProgram shaderProgram_;
		};
	}
}

#endif
