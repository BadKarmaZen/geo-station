using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class will create Game objects, and store them in an internal collection
/// </summary>
public class GameObjectFactory
{
	/// <summary>
	/// Added to freshly instantiated objects, so we can link back
	/// to the correct pool on despawn.
	/// </summary>
	sealed class PoolMember : MonoBehaviour
	{
		public ObjectPool Pool;
	}

	sealed class ObjectPool
  {
    Stack<GameObject> _inactiveObjects;
    int _nextObjectId = 1;
    GameObject _prefabObject;

    public ObjectPool(GameObject prefabObject, int defaultSize = 2)
    {
      _prefabObject = prefabObject;
      _inactiveObjects = new Stack<GameObject>(defaultSize);
    }

		/// <summary>
		///	Spawn an object from the pool or create a new one
		/// </summary>
		public GameObject Spawn(Vector3 pos, Quaternion rot)
		{
			GameObject obj;

			if (_inactiveObjects.Count == 0)
			{
				// We don't have an object in our pool, so we
				// instantiate a whole new object.
				obj = (GameObject)GameObject.Instantiate(_prefabObject, pos, rot);
				obj.name = $"{_prefabObject.name}_{_nextObjectId++}";

				// Add a PoolMember component so we know what pool
				// we belong to.
				obj.AddComponent<PoolMember>().Pool = this;
			}
			else
			{
				// Grab the last object in the inactive array
				obj = _inactiveObjects.Pop();

				if (obj == null)
				{
					// The inactive object we expected to find no longer exists.
					// The most likely causes are:
					//   - Someone calling Destroy() on our object
					//   - A scene change (which will destroy all our objects).
					//     NOTE: This could be prevented with a DontDestroyOnLoad
					//	   if you really don't want this.
					// No worries -- we'll just try the next one in our sequence.

					return Spawn(pos, rot);
				}
			}

			obj.transform.position = pos;
			obj.transform.rotation = rot;
			obj.SetActive(true);
			return obj;
		}

		/// <summary>
		/// Return an object to the inactive pool.
		/// </summary>
		public void Despawn(GameObject obj)
		{
			obj.SetActive(false);
			_inactiveObjects.Push(obj);
		}
	}

	Dictionary<GameObject, ObjectPool> _objectPools = new Dictionary<GameObject, ObjectPool>();

	public void CreatePool(GameObject prefab = null, int amount = 5)
	{
		if (prefab != null && _objectPools.ContainsKey(prefab) == false)
		{
			_objectPools.Add(prefab, new ObjectPool(prefab, amount));
		}
	}

	public GameObject Spawn(GameObject prefab, Vector3 pos, Quaternion rot)
	{
		CreatePool(prefab);

		return _objectPools[prefab].Spawn(pos, rot);
	}

	public void Despawn(GameObject obj)
	{
		PoolMember pm = obj.GetComponent<PoolMember>();
		if (pm == null)
		{
			//Debug.Log ("Object '"+obj.name+"' wasn't spawned from a pool. Destroying it instead.");
			GameObject.Destroy(obj);
		}
		else
		{
			pm.Pool.Despawn(obj);
		}
	}
}
