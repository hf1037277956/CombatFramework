// using System;
// using System.Collections.Generic;
//
//
// namespace EGamePlay
// {
//     public class SubscribeSubject : CombatCore.Core.Entity
//     {
//         public override void Awake(object initData)
//         {
//             Name = initData.GetHashCode().ToString();
//         }
//
//         public SubscribeSubject DisposeWith(CombatCore.Core.Entity entity)
//         {
//             entity.SetChild(this);
//             return this;
//         }
//     }
//
//     public sealed class EventComponent : Component
//     {
//         public override bool DefaultEnable { get; set; }
//         
//         private Dictionary<Type, List<object>> TypeEvent2ActionLists = new();
//         private Dictionary<string, List<object>> FireEvent2ActionLists = new();
//         
//         public static bool DebugLog { get; set; } = false;
//
//         public override void Awake()
//         {
//             DefaultEnable = false;
//         }
//
//         public new T Publish<T>(T TEvent) where T : class
//         {
//             if (!TypeEvent2ActionLists.TryGetValue(typeof(T), out var actionList)) return TEvent;
//             
//             var count = actionList.Count;
//             if (count <= 0) return TEvent;
//             
//             // 倒序遍历
//             for (int i = count - 1; i >= 0; i--)
//             {
//                 // 安全检查：防止越界（如果在回调中 List 被 Clear 了）
//                 if (i >= actionList.Count) continue;
//                         
//                 var action = actionList[i] as Action<T>;
//                 action?.Invoke(TEvent);
//             }
//             
//             return TEvent;
//         }
//         
//         public void FireEvent<T>(string eventType, T entity) where T : CombatCore.Core.Entity
//         {
//             if (!FireEvent2ActionLists.TryGetValue(eventType, out var actionList)) return;
//             // 倒序遍历
//             var count = actionList.Count;
//             if (count <= 0) return;
//             
//             for (int i = count - 1; i >= 0; i--)
//             {
//                 if (i >= actionList.Count) continue;
//
//                 var action = actionList[i] as Action<T>;
//                 action?.Invoke(entity);
//             }
//         }
//         
//         // -----------------------------------------------------------------
//         // 订阅与取消订阅
//         // -----------------------------------------------------------------
//
//         public new void Subscribe<T>(Action<T> action) where T : class
//         {
//             var type = typeof(T);
//             if (!TypeEvent2ActionLists.TryGetValue(type, out var actionList))
//             {
//                 actionList = new List<object>(); // 第一次会分配，后续复用
//                 TypeEvent2ActionLists.Add(type, actionList);
//             }
//             // 查重（可选，为了性能可以去掉，只要保证逻辑不重复添加）
//             if (!actionList.Contains(action)) 
//             {
//                 actionList.Add(action);
//             }
//         }
//         
//         public new void UnSubscribe<T>(Action<T> action) where T : class
//         {
//             if (TypeEvent2ActionLists.TryGetValue(typeof(T), out var actionList))
//             {
//                 actionList.Remove(action);
//             }
//         }
//
//         // public new void UnSubscribe<T>(Action<T> action) where T : class
//         // {
//         //     if (TypeEvent2ActionLists.TryGetValue(typeof(T), out var actionList))
//         //     {
//         //         actionList.Remove(action);
//         //     }
//         //     var e = Parent.Find<SubscribeSubject>(action.GetHashCode().ToString());
//         //     if (e != null)
//         //     {
//         //         Entity.Destroy(e);
//         //     }
//         // }
//         
//         public void OnEvent<T>(string eventType, Action<T> action) where T : CombatCore.Core.Entity
//         {
//             if (!FireEvent2ActionLists.TryGetValue(eventType, out var actionList))
//             {
//                 actionList = new List<object>();
//                 FireEvent2ActionLists.Add(eventType, actionList);
//             }
//             actionList.Add(action);
//         }
//
//         // public void OnEvent<T>(string eventType, Action<T> action) where T : Entity
//         // {
//         //     if (FireEvent2ActionLists.TryGetValue(eventType, out var actionList))
//         //     {
//         //     }
//         //     else
//         //     {
//         //         actionList = new List<object>();
//         //         FireEvent2ActionLists.Add(eventType, actionList);
//         //     }
//         //     actionList.Add(action);
//         // }
//
//         public void OffEvent<T>(string eventType, Action<T> action) where T : CombatCore.Core.Entity
//         {
//             if (FireEvent2ActionLists.TryGetValue(eventType, out var actionList))
//             {
//                 actionList.Remove(action);
//             }
//         }
//         
//         private void Clear()
//         {
//             foreach (var kv in TypeEvent2ActionLists)
//             {
//                 kv.Value.Clear();
//             }
//             
//             foreach (var kv in FireEvent2ActionLists)
//             {
//                 kv.Value.Clear();
//             }
//         }
//         
//         public override void OnDestroy()
//         {
//             Clear();
//         }
//     }
// }