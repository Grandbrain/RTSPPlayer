/// \file PlaybackWidget.cpp
/// \brief Contains classes and functions definitions that provide playback
/// widget implementation.
/// \bug No known bugs.

#include "PlaybackWidget.hpp"

///
namespace Player {

	///
	namespace Playback {

		///
		/// \details
		static constexpr char VERTEX_SHADER_FILENAME[] {
			":/shaders/vs.glsl"
		};

		///
		/// \details
		static constexpr char FRAGMENT_SHADER_FILENAME[] {
			":/shaders/fs.glsl"
		};

		///
		/// \details
		static constexpr char VERTEX_COORDINATE_ATTRIBUTE[] {
			"vertex_coord_in"
		};

		///
		/// \details
		static constexpr char TEXTURE_COORDINATE_ATTRIBUTE[] {
			"texture_coord_in"
		};

		///
		/// \details
		static constexpr char MATRIX_UNIFORM[] {
			"matrix"
		};

		///
		/// \details
		static constexpr float VIEWPORT_VERTICES[] {
			+1.0f, -1.0f, -1.0f, +1.0f,  0.0f,
			-1.0f, -1.0f, -1.0f,  0.0f,  0.0f,
			-1.0f, +1.0f, -1.0f,  0.0f, +1.0f,
			+1.0f, +1.0f, -1.0f, +1.0f, +1.0f
		};

		/// Constructor.
		/// \details
		/// \param[in]	parent	Parent object.
		PlaybackWidget::PlaybackWidget(QWidget* parent)
			: QOpenGLWidget(parent),
			  clearColor_(Qt::black),
			  vertexBuffer_(QOpenGLBuffer::VertexBuffer),
			  colorTexture_(QOpenGLTexture::Target2D),
			  shaderProgram_(this) {

		}

		/// Destructor.
		/// \details
		PlaybackWidget::~PlaybackWidget() {
			destroyResources();
		}

		///
		/// \details
		void PlaybackWidget::initializeGL() {
			initializeOpenGLFunctions();
			initializeResources();

			glEnable(GL_DEPTH_TEST);
			glEnable(GL_CULL_FACE);
		}

		void PlaybackWidget::setImage(const QImage &image) {
			colorTexture_.setData(image);
		}

		///
		/// \details
		void PlaybackWidget::paintGL() {

			glClearColor(clearColor_.redF	(),
						 clearColor_.greenF	(),
						 clearColor_.blueF	(),
						 clearColor_.alphaF	());

			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

			if (shaderProgram_.isLinked()) {
				QMatrix4x4 transformMatrix;
				transformMatrix.ortho(-1.0f, +1.0f, +1.0f, -1.0f, 0.0f, 10.0f);

				shaderProgram_.setUniformValue(MATRIX_UNIFORM, transformMatrix);

				if (colorTexture_.isCreated())
					colorTexture_.bind();

				glDrawArrays(GL_TRIANGLE_FAN, 0, 4);
			}
		}

		///
		/// \details
		/// \param[in]	width
		/// \param[in]	height
		void PlaybackWidget::resizeGL(int width, int height) {
			glViewport(0, 0, width, height);
		}

		///
		/// \details
		void PlaybackWidget::initializeResources() {
			if (!shaderProgram_.addShaderFromSourceFile(
					QOpenGLShader::Vertex, VERTEX_SHADER_FILENAME)		||
				!shaderProgram_.addShaderFromSourceFile(
					QOpenGLShader::Fragment, FRAGMENT_SHADER_FILENAME)	||
				!shaderProgram_.link()									||
				!shaderProgram_.bind()									||
				!vertexBuffer_.create()									||
				!vertexBuffer_.bind()) {
				destroy();
				return;
			}

			vertexBuffer_.allocate(VIEWPORT_VERTICES, sizeof(VIEWPORT_VERTICES));

			shaderProgram_.enableAttributeArray(VERTEX_COORDINATE_ATTRIBUTE);
			shaderProgram_.enableAttributeArray(TEXTURE_COORDINATE_ATTRIBUTE);

			shaderProgram_.setAttributeBuffer(
				VERTEX_COORDINATE_ATTRIBUTE,
				GL_FLOAT,
				0,
				3,
				5 * sizeof(GLfloat)
			);

			shaderProgram_.setAttributeBuffer(
				TEXTURE_COORDINATE_ATTRIBUTE,
				GL_FLOAT,
				3 * sizeof(GLfloat),
				2,
				5 * sizeof(GLfloat)
			);

		}

		///
		/// \details
		void PlaybackWidget::destroyResources() {
			makeCurrent();

			vertexBuffer_.release();
			vertexBuffer_.destroy();

			if (colorTexture_.isCreated() && colorTexture_.isBound())
				colorTexture_.release();

			if (colorTexture_.isCreated())
				colorTexture_.destroy();

			shaderProgram_.release();
			shaderProgram_.removeAllShaders();

			doneCurrent();
		}
	}
}
