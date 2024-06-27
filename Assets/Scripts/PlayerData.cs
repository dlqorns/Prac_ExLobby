using System;
using Unity.Collections; // 메모리 관리가 최적화된 데이터 구조 제공, 리스트 등이 이에 포함.
using Unity.Netcode;

[System.Serializable] // 이 구조체가 직렬화될 수 있도록 지정. Unity Inspector에서 해당 구조체를 편집할 수 있게 해줌.

// PlayerData를 관리하는 구조체. 구조체는 값 타입으로, 주로 간단한 데이터 구조를 표현할 때 사용.
// IEquatable<T>(이하 "A")와 INetworkSerializable(이하 "B") 인터페이스를 상속 받음.
// A를 구현함으로써 구조체 내 인스턴스 비교 가능, B를 구현함으로써 구조체가 네트워크를 통해 직렬화 및 역직렬화될 수 있음.
public struct PlayerData : IEquatable<PlayerData>, INetworkSerializable
{
    public ulong clientId; 
    public int skinIndex;
    public FixedString64Bytes username; // 고정 길이 문자열 타입. 네트워크 전송 시 메모리 관리를 최적화함. 닉네임 관리에 많이 쓰임.

    // 두 PlayerData 인스턴스를 비교하는 메서드.
    public bool Equals(PlayerData other) // IEquatable<PlayerData>의 구현. PlayerData other은 비교할 다른 PlayerData 인스턴스.
    {
        return clientId == other.clientId &&
            skinIndex == other.skinIndex &&
            username == other.username; // 모두 같으면 true 반환.
    }

    // PlayerData 구조체를 네트워크를 통해 직렬화, 역직렬화하는 메서드.
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter // T: 제네릭 타입 매개변수로 IReaderWriter 인터페이스를 구현해야 함.
    {
        serializer.SerializeValue(ref clientId); // clientId를 직렬화하거나 역직렬화함.
        serializer.SerializeValue(ref skinIndex); // skinIndex를 "
        serializer.SerializeValue(ref username); // username을 "
    }
}
