/// \file IOService.hpp
/// \brief Contains declarations of classes and functions to support system I/O
/// services.
/// \bug No known bugs.

#ifndef IOSERVICE_HPP
#define IOSERVICE_HPP

#include <QObject>
#include <asio.hpp>

/// A namespace that contains common classes and functions to support system
/// services.
namespace Common::System {

    class IOService : public QObject
    {
        Q_OBJECT
    public:
        explicit IOService(QObject *parent = nullptr);

    signals:

    };
}

#endif
