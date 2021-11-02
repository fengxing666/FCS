# 开源不易，感谢支持
![支付宝微信](https://raw.githubusercontent.com/Lvwl-CN/FCS/master/doc/zfbwx.png)

# FCS
FCS文件解析帮助类，目前支持DataType为I、F、D 的3.2版本，Mode为L的3.1、3.0版本的FCS文件。3.2版本没有Mode关键字，默认是List存储。详细信息请查看[官方文档](http://flowcyt.sourceforge.net/fcs/fcs32.pdf)

## 使用代码：
```
IEnumerable<FCS> fcslist = FCS.Factory.ReadFile(@"C:\test.fcs");//读取文件中全部数据集
FCS firstDataset = FCS.Factory.ReadFileOneDataset(@"C:\test.fcs",out long nextData,0);//读取文件中第一个数据集
FCS.Factory.SaveToFCS32(@"C:\test1.fcs",fcslist);//保存数据集到文件，3.2版本
FCS.Factory.SaveToFCS31(@"C:\test2.fcs",fcslist);//保存数据集到文件，3.1版本
FCS.Factory.SaveToFCS30(@"C:\test3.fcs",fcslist);//保存数据集到文件，3.0版本
 ```

## FCS：主要输出对象
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | TextSegment | 文本段和补充文本段 | Dictionary< string, string > | 文本段和补充文本段混合在一起，不受FCS文件的文本段长度限制 |
 | AnalysisSegment | 解析段 | Dictionary< string, string > | |
 | Measurements | 数据段 | IList< Measurement > | Measurement是通道类，记录着通道的数据、参数等 |
 | Compensation | 补偿 | Compensation | |

## Measurement：通道参数
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | Name | 名称 | string | 通道名称|
 | BitNumber | 数据位数 | uint | 只支持能被8整除的数，DataType=F时为固定32，DataType=D时为固定64 |
 |ByteNumber|BitNumber/8|int|位数转字节数|
 | Amplification | 放大类型 | Amplification | DataType为D/F时，Amplification固定为 0,0 |
 | Range | 最大值 | ulong | 参数值的区间。只用于DataType=I，因为F和D情况下，会超出这个范围|
 | SuggestedVisualizationScale | 建议可视化范围 | SuggestedVisualizationScale | FCS3.1中新增的可选属性 |
 | OpticalFilter | 光学滤波器名称 | string | |
 | Gain | 增益 | double | 3.2版本只能应用于DataType=I，DataType为F、D时，该值固定为1。3.1、3.0版本不受限制|
 | Wavelength | 激发波长 | string | |
 | Power | 激发功率 | uint | |
 | LongName | 全称 | string | |
 | Detector | 探测器类型 | string | |
 | Voltage | 探测器电压 | double | |
 |DataType|该通道数据类型|DataType|3.2版本新增，区别默认数据类型|
 |Values|该通道的通道值数据集合|IList|内部值可能是double、float、ulong、uint、ushort、byte|
 |AddOneValue(byte[] bytes, ByteOrd byteOrd = ByteOrd.LittleEndian)|向数据集中添加一个数据|void|第一个参数为要添加的数据（字节数组形式），DataType=I时，执行范围（PnR）过滤（v%PnR)|
 |BitMask(T v)|范围过滤（v%PnR)|T|DataType=F、D时不过滤|
 |PnECalculation(ulong value)|PnE对数放大计算|double|只用于DataType=I|
 |PnGCalculation(T value)|PnG线性放大计算（value / PnG）|double|3.2版本只用于DataType=I|
 |ConvertChannelToScaleValue(object obj)|通道值转刻度值|double||
 |GetScaleValues()|获取放大前的刻度值|IList< double >||

## SuggestedVisualizationScale：推荐的可视化范围 PnD
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | Type | 类型 | SuggestedVisualizationScaleType | Linear/Logarithmic |
 | F1 | 最小值 | double | |
 | F2 | 最大值 | double | |

## Amplification：放大类型参数 PnE
 ***公式：v=10^（PowerNumber * xc /（PnR））* ZeroValue**
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | PowerNumber | 10的次方数 | double | |
 | ZeroValue | 0对应的转换值 | double | |

 ## Compensation 补偿（3.0：COMP；3.1、3.2：SPILLOVER)
 | 属性 | 含义 | 类型 | 说明 |
 | --- | --- | --- | --- |
 | MeasurementNames | 参数名称集合，需要与PnN相同 | string[] | |
 | Coefficients | 补偿值集合 | float[][] | |

## 其它
FCS文件格式说明文档可在此下载 [Github](https://github.com/Lvwl-CN/FCS/tree/master/doc)、[Gitee](https://gitee.com/Lvwl-CN/FCS/tree/master/doc)；  
FCS文件可在此下载 [flowrepository](https://flowrepository.org/)

## 更新日志
### 2.0.2
1、规范属性名称  
2、Measurement类实现INotifyPropertyChanged接口

### 2.0.1
1、添加通道值转刻度值方法  
2、数据解析时，实例化数组时设置数组的Capacity属性  
3、TOT属性类型更改为int

### 2.0.0
1、更改输出对象FCS，输出对象无关文件信息（版本、段起止位置等）  
2、添加3.2版本的支持  
3、添加保存功能  
4、修复一些bug

### 1.0.0
1、添加读取和解析文件功能


