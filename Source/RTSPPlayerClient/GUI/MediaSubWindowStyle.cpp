/// \file MediaSubWindowStyle.cpp
/// \brief Contains definitions of classes and functions for customizing
/// media subwindows.
/// \bug No known bugs.

#include "MediaSubWindow.hpp"
#include "MediaSubWindowStyle.hpp"

/// A namespace that contains GUI classes and functions.
namespace GUI {

    /// Constructs a media subwindow style.
    /// \details Constructs a media subwindow style for overriding behavior in
    /// the specified \a style.
    /// \param[in]  style   Source style.
    MediaSubWindowStyle::MediaSubWindowStyle(QStyle* style)
        : QProxyStyle(style) {

    }

    /// Draws complex control.
    /// \details
    /// \param[in]  control Complex control.
    /// \param[in]  option  Control style parameters.
    /// \param[in]  painter Painter object.
    /// \param[in]  widget  Widget object.
    void MediaSubWindowStyle::drawComplexControl(ComplexControl control,
                                                 const QStyleOptionComplex* option,
                                                 QPainter* painter,
                                                 const QWidget* widget) const {

        auto customOption = option;
        QStyleOptionTitleBar customTitleBarOption;

        if (control == QStyle::ComplexControl::CC_TitleBar) {
            auto mediaSubWindow = qobject_cast<const MediaSubWindow*>(widget);

            if (mediaSubWindow) {
                auto titleBarColor = mediaSubWindow->titleBarColor();
                auto titleBarOption =
                    qstyleoption_cast<const QStyleOptionTitleBar*>(option);

                if (titleBarColor.isValid() && titleBarOption) {
                    customTitleBarOption = *titleBarOption;

                    if (customTitleBarOption.palette
                            .currentColorGroup() != QPalette::Disabled)
                        customTitleBarOption.palette
                            .setBrush(QPalette::Highlight, titleBarColor);

                    customOption = &customTitleBarOption;
                }
            }
        }

        QProxyStyle::drawComplexControl(control, customOption, painter, widget);
    }
}
