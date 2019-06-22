# TianCheng.DAL.MongoDB

目标架构为：.NET Standard 2.0

MongoDB 的数据库访问操作，包括连接数据库并缓存数据库操作服务、日志处理、MongoDB的UTC时间转换、以及常用的增删改查操作。

## 如何在WebApi中使用

1. 在`Startup`的`ConfigureServices`中增加一行：`services.TianChengMongoDBInit(Configuration);`
2. 在`Startup`的`Configure`中增加一行：`app.TianChengDALInit(Configuration);`

示例

  ```csharp

    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);

            // 注册MongoDB数据库模块配置信息
            services.TianChengMongoDBInit(Configuration);               // 此处新增一行
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            // 初始化数据库连接配置
            app.TianChengDALInit(Configuration);                      // 此处新增一行
        }
    }
```

## 数据库连接说明

### 1. 数据库连接配置

  在appsettings.json中配置`DBConnection`节点。

  ``` json
    "DBConnection": [
      {
        "Name": "default",
        "ServerAddress": "mongodb://localhost:27017",
        "Database": "samples",
        "Type": "MongoDB"
      },
      {
        "Name": "debug",
        "ServerAddress": "mongodb://localhost:27017",
        "Database": "rt_test",
        "Type": "MongoDB"
      }
    ]
  ```

  > DBConnection下可以有多组数据库连接。需要以不同的Name区别，如果没有Name或Name为空时，默认使用default命名。
  >
  > 请将类型设置为MongoDB

### 2. 在对应的数据操作对象中通过`DBMapping`特性来指明操作那个库。如果不指定连接名称，默认对default连接进行操作

  ```csharp
      /// <summary>
      /// 按数据库连接名debug的配置操作表test_demo
      /// </summary>
      [DBMapping("test_demo", "debug")]
      class DemoDebugDAL : MongoOperation<DemoInfo>
      {

      }
  ```

> MongoOperation指定一个操作对象，即操作的数据结构。这个对象需要继承`BusinessMongoModel`

  ```csharp
    /// <summary>
    /// Demo对象实体
    /// </summary>
    public class DemoInfo : BusinessMongoModel
    {
        [BsonElement("Name")]
        public string Name { get; set; }
        /// <summary>
        /// BsonIgnore 可以将忽略属性，不保存进数据库
        /// </summary>
        [BsonIgnore]
        public string Code { get; set; }
        /// <summary>
        /// BsonElement可以为属性起个别名存放
        /// </summary>
        [BsonElement("test_date")]
        public DateTime Date { get; set; }
    }
  ```

## 日志说明

通过`TianCheng.Model`组件包使用[Serilog](https://serilog.net/)作为日志处理的工具。已添加操作对象`MongoLog`。`DBLog`是按固定的配置写日志。

`MongoLog`的日志配置如下：

1. 控制台输出Warning级别以上的信息；
2. Debug窗口输出为全输出；
3. 文件输出为Warning级别以上的，文件名格式为`Logs/TianCheng.DBOperation-{Date}.txt`；(其实是直接调用TianCheng.DAL中的文件路径配置)
4. MongoDB数据库记录级别为Error，使用库为配置的default连接信息；存储集合名称为system_log。
5. 邮件发送级别为Fatal，收件账号可以通过`ToEmail`属性设置。

## 常用数据库方法说明

|方法定义   |  说明 |
|:-----------------------|:-------------|
| void Save(T entity)    | 保存对象，根据ID是否为空来判断是新增还是修改操作 |
| void InsertObject(T entity)| 插入单条新数据 |
| void InsertRange(IEnumerable &lt;T&gt; entities)|插入多条新数据|
| async void InsertAsync(T entity) | 异步插入单条数据  |
| async void InsertAsync(IEnumerable &lt;T&gt; entities)|异步插入多条数据   |
| bool UpdateObject(T entity) | 更新单条数据|
| void UpdateRange(IEnumerable &lt;T&gt; entities)|更新多条数据 |
| async Task&lt;bool&gt; UpdateAsync(T entity)|异步更新单条数据 |
| async Task&lt;bool&gt; UpdateAsync(IEnumerable &lt;T&gt; entities)|异步更新多条数据 |
| bool UpdatePropertyById(string id, string propertyName, object propertyValue) |根据ID更新一个属性 |
| bool UpdatePropertyById(ObjectId id, string propertyName, object propertyValue) |根据ID更新一个属性 |
| bool UpdatePropertyById(string id, UpdateDefinition &lt;T&gt; upProperty)| 按ID更新多个属性|
| bool UpdatePropertyById(ObjectId objId, UpdateDefinition &lt;T&gt; upProperty)| 按ID更新多个属性|
| T SearchById(string id)|  根据id查询对象 |
| T SearchByTypeId(ObjectId id)| 根据id查询对象 |
| List&lt;T&gt; SearchByIds(IEnumerable&lt;string&gt; ids)| 根据ID列表获取对象集合 |
| IQueryable&lt;T&gt; Queryable()| 获取当前集合的查询链式接口 |
| bool HasRepeat(ObjectId objectId, string prop, string val, bool ignoreDelete = true)|查看指定属性值在表中是否有重复项 |
| List&lt;T&gt; Search(FilterDefinition&lt;T&gt; filter, SortDefinition&lt;T&gt; sort)| 根据Mongodb的查询条件查询 |
| List&lt;T&gt; Search(FilterDefinition&lt;T&gt; filter, QueryInfo queryInfo)| 根据Mongodb的查询条件查询 ( 分页 )|
| IEnumerable&lt;T&gt; Search(T entity)|  根据实体来查询|
| List&lt;R&gt; Aggregate&lt;R&gt;(PipelineDefinition&lt;T, R&gt; pipeline)|  通过Aggregate统计查询|
| void RemoveSearch(T entity)| 将对象内容作为查询条件来物理删除数据 |
| T RemoveObject(T entity)| 物理删除对象|
| bool RemoveByIdList(IEnumerable&lt;string&gt; ids)| 根据ID列表 物理删除一组数据|
| bool RemoveByTypeIdList(IEnumerable&lt;ObjectId&gt; ids)| 根据ID列表 物理删除一组数据|
| T RemoveById(string id)| 根据ID 物理删除数据|
| T RemoveByTypeId(ObjectId id)| 根据ID 物理删除数据|
| bool DeleteById(string id) |根据ID 逻辑删除数据 |
| bool DeleteByTypeId(ObjectId objId)| 根据ID 逻辑删除数据 |
| bool DeleteByIdList(IEnumerable&lt;string&gt; ids)| 根据ID列表 逻辑删除一组数据 |
| bool DeleteByTypeIdList(IEnumerable&lt;ObjectId&gt; ids)| 根据ID列表 逻辑删除一组数据|
| bool UndeleteById(string id) | 根据ID 还原被逻辑删除的数据 |
| bool UndeleteByTypeId(ObjectId objId)|根据ID 还原被逻辑删除的数据 |
| bool UndeleteByIdList(IEnumerable&lt;string&gt; ids)| 根据ID列表 还原被逻辑删除的一组数据|
| bool UndeleteByTypeIdList(IEnumerable&lt;ObjectId&gt; ids)|根据ID列表 还原被逻辑删除的一组数据 |
| void Drop()|删除表（集合） |
| async void DropAsync()|删除表（集合） 异步 |
