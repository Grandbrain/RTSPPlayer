/// \file MediaSubWindowStyle.hpp
/// \brief Contains declarations of classes and functions for customizing
/// media subwindows.
/// \bug No known bugs.

#ifndef MEDIASUBWINDOWSTYLE_HPP
#define MEDIASUBWINDOWSTYLE_HPP

#include <QProxyStyle>
#include <QStyleOptionComplex>

/// A namespace that contains GUI classes and functions.
namespace GUI {

    /// A class that provides a custom style for media subwindows.
    class MediaSubWindowStyle : public QProxyStyle {

        Q_OBJECT

    public:

        /// Constructs a media subwindow style.
        /// \param[in]  style   Source style.
        explicit MediaSubWindowStyle(QStyle* style = nullptr);

    public:

        /// Draws complex control.
        /// \param[in]  control Complex control.
        /// \param[in]  option  Control style parameters.
        /// \param[in]  painter Painter object.
        /// \param[in]  widget  Widget object.
        void drawComplexControl(ComplexControl control,
                                const QStyleOptionComplex* option,
                                QPainter* painter,
                                const QWidget* widget = nullptr) const override;
    };
}

#endif
