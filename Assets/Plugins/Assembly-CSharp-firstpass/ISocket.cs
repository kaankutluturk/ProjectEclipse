internal interface ISocket
{
	void Open();

	void Disconnect(bool GGONLJPAABO);

	void OnPacket(Packet NPKADBPBKIG);

	void EmitEvent(SocketIOEventType LFLGCDNKNJI, params object[] LKIOKGCNKHE);

	void EmitEvent(string DOPHKKGNAEF, params object[] LKIOKGCNKHE);

	void EmitError(SocketIOErrors GNKCGOGKAEK, string CKEHOEGLMBM);
}
