# OfficeAutomationClient

写这个工具只是想方便查询考勤以及学习一些东西，打发一下时间。
废话也不多说了，不太擅长。

列举一下用到的东西，也算不上什么牛X的技术，欢迎指教。

- 验证码处理：从网络流得到Bitmap，转成ImageSource显示；对Bitmap做灰度化、去噪声、二值化，然后交给Ocr引擎（tesseract）识别，得到验证码。
- 用户名密码保存：使用了[CredentialManagement](https://www.nuget.org/packages/CredentialManagement/)。注意密码不要明文保存，我使用了`ProtectedData`对密码做加密处理。
- 模拟请求：验证码获取、登录以及考勤记录查询都是通过 ~~`HttpWebRequest`模拟的，我的工具库[CommonUtility](https://www.nuget.org/packages/Net.Liap.CommonUtility/)封装了它的一些基本操作，如保存Cookie，Post参数等。代码风格有那么一点点函数式编程的意思？~~ [HttpClient](https://www.nuget.org/packages/Microsoft.Net.Http/)类处理的（2018年6月29日更改）。
- Retry策略：模拟请求得到的结果有时可能不正确(429或其他错误)，这时就需要用到Retry策略。曾经想写一个放到[CommonUtility](https://www.nuget.org/packages/Net.Liap.CommonUtility/)中壮大我的工具库的，之后发现确实不是件易事，然后我找到了[Polly.Net40Async](https://www.nuget.org/packages/Polly.Net40Async/)。之前也有个类似的事情发生，当时动脑筋想弄个Json生成CSharp类的工具，到现在还没有开始。对此我的总结就是：凡是涉及到编译原理的，都不是我的菜。如果你们有学习编译原理的好的方法，请务必指点一下我这个菜鸟。
- Html解析：这个不用多说，有[HtmlAgilityPack](https://www.nuget.org/packages/HtmlAgilityPack/)这个强大的库。
- 数据库：选用`EntityFramework`+`SQLite`，在配置上花了一些功夫。开发模式选了`Code First`、`Model First`、`Database First`中的第一个，为了支持这种模式，需要安装[SQLite.CodeFirst](https://www.nuget.org/packages/SQLite.CodeFirst/)。
- 考勤记录展示：选择[中华万年历](http://yun.rili.cn/wnl/index.html)作为基本界面，通过WPF的WebBrowser控件加载它，并注入简单的js和css，实现了考勤记录的展示。
- 关于效率优化：在启动时对`HttpClient`和`EntityFramework`进行‘预热’；使用`MemoryCache`缓存考勤记录结果。