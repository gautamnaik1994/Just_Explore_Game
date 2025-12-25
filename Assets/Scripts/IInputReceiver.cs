
using UnityEngine;

public interface IInputReceiver
{
     void Move(Vector3 direction);
     void Jump();
     void Custom();
}
