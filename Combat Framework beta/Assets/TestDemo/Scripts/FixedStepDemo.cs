using UnityEngine;

public class FixedStepDemo : MonoBehaviour
{
    [Header("表现层引用")]
    public Transform cubeVariable; // 红块：代表传统 Update
    public Transform cubeFixed;    // 蓝块：代表你的战斗框架

    [Header("战斗框架配置")]
    public float speed = 10f;           // 移动速度
    public float boundary = 6f;         // 左右反弹边界
    public float logicTickRate = 30f;   // 你的战斗逻辑帧率 (30Hz)
    
    // --- 传统做法的变量 ---
    private Vector3 variablePos;
    private float variableDir = 1f;

    // --- 战斗框架逻辑层的纯数据 (不依赖 Transform) ---
    private float fixedDeltaTime;
    private float accumulator = 0f;
    private int maxStepsPerFrame = 5;   // 防死亡螺旋限制

    private Vector3 logicPrevPos;       // 上一逻辑帧位置
    private Vector3 logicCurrPos;       // 当前逻辑帧位置
    private float fixedDir = 1f;        // 逻辑层移动方向

    void Start()
    {
        // 1. 初始化框架参数
        fixedDeltaTime = 1f / logicTickRate;
        
        // 2. 初始化逻辑状态 (纯数据)
        logicPrevPos = cubeFixed.position;
        logicCurrPos = cubeFixed.position;

        variablePos = cubeVariable.position;
    }

    void Update()
    {
        // ========================================================
        // 0. 人为制造“低端机卡顿”或“垃圾回收(GC)尖峰”
        // 按下空格键，强制主线程挂起 200 毫秒！
        // ========================================================
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.LogWarning("模拟低端机严重卡顿 200ms!");
            System.Threading.Thread.Sleep(200); 
        }

        float dt = Time.deltaTime;

        // ========================================================
        // 1. 传统 Update 做法 (反面教材)
        // ========================================================
        variablePos += Vector3.right * variableDir * speed * dt;
        
        // 朴素的边界反弹检测
        if (Mathf.Abs(variablePos.x) > boundary)
        {
            variableDir *= -1; // 调转方向
        }
        cubeVariable.position = variablePos;


        // ========================================================
        // 2. 你的战斗框架 Tick (逻辑与表现分离)
        // ========================================================
        accumulator += dt;
        int stepCount = 0;

        // 只要积累的时间够一个逻辑帧，就推演一次纯逻辑
        while (accumulator >= fixedDeltaTime && stepCount < maxStepsPerFrame)
        {
            // A. 备份上一帧状态
            logicPrevPos = logicCurrPos;

            // B. 推演当前帧状态 (只操作数据，绝对不碰 Transform)
            logicCurrPos += Vector3.right * fixedDir * speed * fixedDeltaTime;

            // C. 逻辑层的边界检测 (步长固定，判定绝对精准)
            if (Mathf.Abs(logicCurrPos.x) > boundary)
            {
                fixedDir *= -1; 
                // 严谨的做法：算出超出的距离并折返，这里为了简易Demo保持简单
            }

            // D. 消耗时间
            accumulator -= fixedDeltaTime;
            stepCount++;
        }

        // 防御性重置
        if (stepCount >= maxStepsPerFrame) accumulator = 0f;

        // ========================================================
        // 3. 表现层插值渲染 (平滑画面)
        // ========================================================
        // 算出当前时间在两个逻辑帧之间的百分比
        float alpha = accumulator / fixedDeltaTime;
        
        // 让蓝块的实际 Transform 根据 Alpha 平滑过渡
        cubeFixed.position = Vector3.Lerp(logicPrevPos, logicCurrPos, alpha);
    }
}