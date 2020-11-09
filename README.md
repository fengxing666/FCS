# FCS
FCS文件解析帮助类，目前支持Mode为List，DataType为Integer、Float、Double 的3.1和3.0版本的FCS文件，暂时不支持保存操作

## 使用代码：
```
IEnumerable<IFCS> fcslist = FCS.Factory.ReadFCSFile(@"C:\test.fcs");
 ```
读取文件并返回FCS对象集合，通常一个FCS文件只有一个数据集

## IFCS：主要输出对象，实现类有FCS3_0、FCS3_1
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | TextSegment | 文本段和补充文本段 | Dictionary< string, string > | 文本段和补充文本段混合在一起，不受FCS文件的文本段长度限制 |
 | AnalysisSegment | 解析文本段 | Dictionary< string, string > | |
 | DataSegment | 数据段 | IEnumerable< IList > | IList里面的内容可能是double、float、int |
 | Params | 参数集合 | IList< Param > | FCS文件里面记录的参数集合，PAR就是该集合的长度，Param类包含了名称、放大类型、增益等属性 |
 | Version | FCS文件的版本 | string | FCS3.0/FCS3.1 |
 | PAR | 参数个数 | uint | 决定Params的长度 |
 | TOT | 数据量 | ulong | 数据段共有多少数据量 |
 | NextData | 下一个数据集起点位置 | long | 一个FCS文件可以有多个数据集，在第一个数据集中需要记录第二个数据的起点位置 |

## RecommendsVisualizationScale：推荐的可视化范围 PnD
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | Type | 类型 | RecommendsVisualizationScaleType | Linear/Logarithmic |
 | F1 | 最小值 | double | |
 | F1 | 最大值 | double | |

## Amplification：放大类型参数 PnE
 ***公式：v=10^（PowerNumber * xc /（PnR））* ZeroValue**
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | PowerNumber | 10的次方数 | double | |
 | ZeroValue | 0对应的转换值 | double | |

## Param：FCS参数
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | PnN | 名称 | string | |
 | PnB | 数据位数 | uint | DataType为ASCII时，表示字节数 |
 | PnE | 放大类型 | string | DataType为double/float时，PnE应固定为 0,0 |
 | AmplificationValue | 放大类型 | Amplification | PnE解析后的类 |
 | PnR | 最大值 | uint | 参数值的最大值，补偿等操作后可能会超过这个值 |
 | PnD | 建议可视化范围 | string | FCS3.1中新增的可选属性 |
 | RecommendsVisualizationScaleValue | 建议可视化范围 | RecommendsVisualizationScale | PnD解析后的类 |
 | PnF | 光学滤波器名称 | string | |
 | PnG | 增益 | double | |
 | PnL | 激发波长 | string | |
 | PnO | 激发功率 | uint | |
 | PnP | 发散光的百分比 | uint | |
 | PnS | 全称 | string | |
 | PnT | 探测器类型 | string | |
 | PnV | 探测器电压 | double | |
 
数据格式文件可在此下载 [Github](https://github.com/Lvwl-CN/FCS/tree/master/doc)、[Gitee](https://gitee.com/Lvwl-CN/FCS/tree/master/doc)

FCS文件可在此下载 [flowrepository](https://flowrepository.org/)
