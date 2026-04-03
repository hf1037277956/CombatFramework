// using System;
// using System.Collections.Generic;
// using System.Linq;
// using UnityEngine;
//
//
// namespace EGamePlay
// {
//     public sealed class MasterEntity : CombatCore.Core.Entity
//     {
//         public Dictionary<Type, List<CombatCore.Core.Entity>> Entities { get; } = new(50);
//         public List<Component> AllComponents { get; } = new(50);
//         //public List<UpdateComponent> UpdateComponents { get; private set; } = new List<UpdateComponent>(50);
//
//         public static MasterEntity Instance { get; private set; }
//
//
//         private MasterEntity()
//         {
//             
//         }
//
//         public static void Create()
//         {
//             if (Instance == null)
//             {
//                 Instance = new MasterEntity();
// #if UNITY_EDITOR
//                 // Instance.AddComponent<GameObjectComponent>();
//                 // UnityEngine.GameObject.DontDestroyOnLoad(Instance.GetComponent<GameObjectComponent>().GameObject);
// #endif
//             }
//         }
//
//         public static void Destroy()
//         {
//             if(Instance == null)
//                 return;
//             Destroy(Instance);
//             Instance = null;
//         }
//
//         public void Clear()
//         {
//             foreach (var component in AllComponents)
//             {
//                 Component.Destroy(component);
//             }
//
//             foreach (var entityList in Entities.ToList())
//             {
//                 foreach (var entity in entityList.Value)
//                 {
//                     CombatCore.Core.Entity.Destroy(entity);
//                 }
//             }
//             
//             AllComponents.Clear();
//             Entities.Clear();
//         }
//
//         public override void Update(float deltaTime)
//         {
//             if (AllComponents.Count == 0)
//             {
//                 return;
//             }
//             for (int i = AllComponents.Count - 1; i >= 0; i--)
//             {
//                 var item = AllComponents[i];
//                 if (item.IsDisposed)
//                 {
//                     AllComponents.RemoveAt(i);
//                     continue;
//                 }
//                 if (item.Disable)
//                 {
//                     continue;
//                 }
//                 item.Update(deltaTime);
//             }
//         }
//         
//         // 打印Entities
//         public void Print()
//         {
//             foreach (var item in Entities)
//             {
//                 Debug.Log($"Key:{item.Key.Name} Count:{item.Value.Count}");
//                 foreach (var v in item.Value)
//                 {
//                     Debug.Log($"    {v.GetType().Name}={v.Id}");
//                 }
//             }
//         }
//         
//         // 添加Entity
//         public void AddEntity(Type entityType, CombatCore.Core.Entity entity)
//         {
//             if (!Entities.ContainsKey(entityType))
//             {
//                 Entities.Add(entityType, new List<CombatCore.Core.Entity>());
//             }
//             Entities[entityType].Add(entity);
//         }
//         // 删除实体
//         public void RemoveEntity(CombatCore.Core.Entity entity)
//         {
//             var entityType = entity.GetType();
//             if (!Entities.ContainsKey(entityType))
//             {
//                 return;
//             }
//             Entities[entityType].Remove(entity);
//         }
//         
//         /// <summary>
//         /// 删除某个类型的所有实体
//         /// </summary>
//         /// <param name="entityType"></param>
//         public void RemoveEntities(Type entityType)
//         {
//             if (!Entities.ContainsKey(entityType))
//             {
//                 return;
//             }
//             Entities[entityType].Clear();
//         }
//     }
// }