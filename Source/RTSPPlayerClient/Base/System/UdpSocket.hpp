#ifndef UDPSOCKET_HPP
#define UDPSOCKET_HPP

#include <QObject>

class UdpSocket : public QObject
{
    Q_OBJECT
public:
    explicit UdpSocket(QObject *parent = nullptr);

signals:

};

#endif // UDPSOCKET_HPP
