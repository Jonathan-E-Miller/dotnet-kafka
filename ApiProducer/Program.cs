using ApiProducer;
using ApiProducer.Interfaces;
using Confluent.Kafka;
using Persistence.Mongo;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Scoped objects are the same within a request, but different across different requests.
// Singleton objects are always the same
// Transiest objects are different for each request
builder.Services.AddLogging(o => o.AddSeq("http://seq"));
builder.Services.AddSingleton<IKafkaContainer, KafkaContainer>();
builder.Services.AddSingleton(typeof(IMongoRepository<>), typeof(MongoRepository<>));
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.AddSingleton<IAdminClient>(x => new AdminClientBuilder(new AdminClientConfig { BootstrapServers = builder.Configuration.GetSection("Kafka")["bootstrap.servers"] }).Build());
builder.Services.AddSingleton<IProducer<string, string>>(
    new ProducerBuilder<string, string>(
        builder.Configuration.GetSection("Kafka")
        .GetChildren()
        .Select(x => new KeyValuePair<string, string>(x.Key, x.Value)).ToList()).Build());

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();  

app.UseAuthorization();

app.MapControllers();

app.Run();
