Namespace API20.Base
    Public Class RpcRequest
        Inherits BaseRequest
        Dim m_d As SuccessDelegate
        Property [Delegate]() As SuccessDelegate
            Get
                Return m_d
            End Get
            Set(ByVal value As SuccessDelegate)
                m_d = value
            End Set
        End Property


        Property RPC() As WebRequests.XMLRpc
            Get
                Return m_rpc
            End Get
            Set(ByVal value As WebRequests.XMLRpc)
                m_rpc = value
            End Set
        End Property
        Sub New(ByVal rpc As WebRequests.XMLRpc, ByVal d As SuccessDelegate)
            m_rpc = rpc
            m_d = d
        End Sub

        Overrides Sub Start()
            MyBase.Post(RPC.toString)
        End Sub
        Delegate Sub SuccessDelegate(ByVal elem As Xml.XmlElement)

        Protected Overrides Sub Success(ByVal elem As System.Xml.XmlElement)
            m_d.Invoke(elem)
        End Sub
    End Class
End Namespace