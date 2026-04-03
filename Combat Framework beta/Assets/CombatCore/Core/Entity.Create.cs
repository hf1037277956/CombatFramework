// using System;
// using System.Collections.Generic;
// using System.Diagnostics;
// using System.Linq;
//
// namespace EGamePlay
// {
//     [System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
//     sealed class EnableUpdateAttribute : Attribute
//     {
//         public EnableUpdateAttribute()
//         {
//         }
//     }
//
//     public abstract partial class Entity
//     {
//         public static MasterEntity Master => MasterEntity.Instance;
//         public static bool EnableLog { get; set; } = false;
//
//         public static CombatCore.Core.Entity NewEntity(Type entityType, bool isFromPool)
//         {
//             CombatCore.Core.Entity entity;
//             if (isFromPool)
//             {
//                 entity = EntityObjectPool.Instance.Fetch(entityType);
//             }
//             else
//             {
//                 entity = Activator.CreateInstance(entityType) as CombatCore.Core.Entity; 
//             }
//             
//             entity.InstanceId = IdFactory.NewInstanceId();
//             entity.Id = entity.InstanceId;
//             entity.IsFromPool = isFromPool;
//             
//             Master?.AddEntity(entityType, entity);
//             
//             return entity;
//         }
//         
//         // ----------------------------------------------------------------------
//         // 对象池版本：CreateFromPool
//         // ----------------------------------------------------------------------
//     
//         public static T CreateFromPool<T>() where T : CombatCore.Core.Entity
//         {
//             return (T)CreateFromPool(typeof(T));
//         }
//         
//         public static T CreateFromPool<T>(object initData) where T : CombatCore.Core.Entity
//         {
//             return (T)CreateFromPool(typeof(T), initData);
//         }
//
//         public static CombatCore.Core.Entity CreateFromPool(Type entityType)
//         {
//             var entity = NewEntity(entityType, true);
//             //if (EnableLog) Log.Debug($"CreateFromPool {entityType.Name}={entity.Id}");
//             SetupEntity(entity, Master);
//             return entity;
//         }
//
//         public static CombatCore.Core.Entity CreateFromPool(Type entityType, object initData)
//         {
//             var entity = NewEntity(entityType, true);
//             //if (EnableLog) Log.Debug($"CreateFromPool {entityType.Name}={entity.Id}, {initData}");
//             SetupEntity(entity, Master, initData);
//             return entity;
//         }
//         
//         // ----------------------------------------------------------------------
//         // 非对象池版本：Create
//         // ----------------------------------------------------------------------
//
//         public static T Create<T>() where T : CombatCore.Core.Entity
//         {
//             return (T)Create(typeof(T));
//         }
//
//         public static T Create<T>(object initData) where T : CombatCore.Core.Entity
//         {
//             return (T)Create(typeof(T), initData);
//         }
//
//         public static CombatCore.Core.Entity Create(Type entityType)
//         {
//             var entity = NewEntity(entityType, false); // isFromPool = false
//             //if (EnableLog) Log.Debug($"Create {entityType.Name}={entity.Id}");
//             SetupEntity(entity, Master);
//             return entity;
//         }
//
//         public static CombatCore.Core.Entity Create(Type entityType, object initData)
//         {
//             var entity = NewEntity(entityType, false); // isFromPool = false
//             //if (EnableLog) Log.Debug($"Create {entityType.Name}={entity.Id}, {initData}");
//             SetupEntity(entity, Master, initData);
//             return entity;
//         }
//         
//         // ----------------------------------------------------------------------
//         // 特殊组：CreateNoParent
//         // ----------------------------------------------------------------------
//
//         public static T CreateNoParent<T>(object initData) where T : CombatCore.Core.Entity
//         {
//             return CreateNoParent(typeof(T), initData) as T;
//         }
//         
//         public static CombatCore.Core.Entity CreateNoParent(Type entityType, object initData)
//         {
//             var entity = NewEntity(entityType, false);
//             //if (EnableLog) Log.Debug($"CreateWithParent {entityType.Name}={entity.Id}");
//             entity.Awake(initData);
//             entity.Start(initData);
//             return entity;
//         }
//
//         // ----------------------------------------------------------------------
//         // 初始化辅助方法
//         // ----------------------------------------------------------------------
//
//         private static void SetupEntity(CombatCore.Core.Entity entity, CombatCore.Core.Entity parent)
//         {
//             parent.SetChild(entity);
//             entity.Awake();
//             entity.Start();
//         }
//
//         private static void SetupEntity(CombatCore.Core.Entity entity, CombatCore.Core.Entity parent, object initData)
//         {
//             parent.SetChild(entity);
//             entity.Awake(initData);
//             entity.Start(initData);
//         }
//
//         // ----------------------------------------------------------------------
//         // 销毁逻辑
//         // ----------------------------------------------------------------------
//         
//         public static void Destroy(CombatCore.Core.Entity entity)
//         {
//             // if (entity.IsDestroying) return;
//             // entity.IsDestroying = true;
//             
//             try
//             {
//                 entity.OnDestroy();
//             }
//             catch (Exception e)
//             {
//                 //Log.Error(e);
//             }
//             entity.Dispose();
//         }
//     }
// }
