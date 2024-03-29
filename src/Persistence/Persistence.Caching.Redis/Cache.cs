﻿using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NLog;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Persistence.Caching.Redis
{
    public class Cache : ICache
    {
        private readonly ConnectionMultiplexer redis;
        private readonly IDatabase db;
        private readonly DatabaseType _dbType;
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private static ConfigurationOptions options;

        private static int _port;
        private static string _host;


        static Cache()
        {
            var configuration = new ConfigurationBuilder()
              .AddJsonFile("appSettings.json").Build();
            _host = configuration["Redis:Default:HostName"];
            _port = Convert.ToInt32(configuration["Redis:Default:Port"]);
          
        }

        public Cache(DatabaseType dbType = DatabaseType.System)
        {
            _dbType = dbType;
            try
            {
                logger.Info($"Trying to connect Reis on {_host}:{_port}");
                options = new()
                {
                    EndPoints = { { _host, Convert.ToInt32(_port) } },
                    AllowAdmin = true
                };
                redis = ConnectionMultiplexer.Connect(options);
            }
            catch (RedisConnectionException ex)
            {
               // logger.Error(ex);
                logger.Info($"Failed to connect redis");
                logger.Info($"Rettrying to connect redis on container host");
                options = new()
                {
                    EndPoints = { { "redis" } },
                    AllowAdmin = true
                };
                redis = ConnectionMultiplexer.Connect(options);           
            }
            
            logger.Info($"Redis succesfully connected");
            db = redis.GetDatabase((int)dbType);
        }

        public TResult Get<TResult>(string key,bool getThanDelete = false)
        {
            var redisKey = new RedisKey(key);
            var data = getThanDelete ? db.StringGetDelete(redisKey) : db.StringGet(redisKey);
            return data != default ? JsonConvert.DeserializeObject<TResult>(data.ToString()) : default;
        }

        public void Set(string key, object value)
        {
            string valueToSet = JsonConvert.SerializeObject(value);
            db.StringSet(new RedisKey(key), new RedisValue(valueToSet), TimeSpan.FromDays(90));
        }

        public List<string> GetAllkeys()
        {
            List<string> listKeys = new List<string>();
            var keys = redis.GetServer(options.EndPoints.First()).Keys((int)_dbType);
            listKeys.AddRange(keys.Select(key => (string)key).ToList());
            return listKeys;
        }

        public void PurgeDatabase()
        {
            redis.GetServer(options.EndPoints.First()).FlushDatabase((int)_dbType);
        }

        protected IEnumerable<RedisKey> GetALLKeys()
        {
            return redis.GetServer(options.EndPoints.First()).Keys((int)_dbType);
        }

        protected void Remove(RedisKey key)
        {
            db.KeyDelete(key);
        }
    }
}