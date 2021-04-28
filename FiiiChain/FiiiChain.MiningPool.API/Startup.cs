﻿using FiiiChain.MiningPool.API.DataPools;
using FiiiChain.MiningPool.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.PlatformAbstractions;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.IO;

namespace FiiiChain.MiningPool.API
{
    public class Startup
    {
        public static Func<List<string>> MinerListAction;
        public static List<Miners> Pool_Miners = null;
        public static long Pool_Miners_UpdateTime = 0;

        public Startup(IConfiguration configuration)
        {
            ServerPool.Default.Start();
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            #region  添加SwaggerUI

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Info

                {
                    Title = "FiiiChain miningPool API接口文档",
                    Version = "v1",
                    Description = "FiiiChain miningPool API接口文档",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "wangshibang", Email = "Fiii@fiii.com", Url = "" }
                });
                options.IgnoreObsoleteActions();
                options.DocInclusionPredicate((docName, description) => true);
                options.IncludeXmlComments(Path.Combine(PlatformServices.Default.Application.ApplicationBasePath, "FiiiChain.MiningPool.API.xml"));
                options.DescribeAllEnumsAsStrings();
                //options.TagActionsBy(api => api.HttpMethod); //根据Http请求排序
                //options.OperationFilter<BaseController>(); // 添加httpHeader参数
            });

            #endregion
            services.AddMemoryCache();
            services.AddDistributedRedisCache(options =>
            {
                options.Configuration = "r-3ns10cea37457624.redis.rds.aliyuncs.com,abortConnect=False";                                                                                                             
            });
            //返回区分大小写
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddJsonOptions(option => { option.SerializerSettings.ContractResolver = new DefaultContractResolver(); });
            //services.AddJsonRpc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            #region 使用SwaggerUI

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "FiiiChain miningPool API V1");
            });

            #endregion

            app.UseMvc();
            /*
            app.UseManualJsonRpc(builder =>
            {
                //builder.RegisterController<UtxoController>();
                //builder.RegisterController<TransactionController>();
                //builder.RegisterController<AccountController>();
                //builder.RegisterController<AddressBookController>();
                //builder.RegisterController<PaymentRequestController>();
                //builder.RegisterController<WalletController>();
                //builder.RegisterController<MemPoolController>();
                //builder.RegisterController<NetworkController>();
            });
            */
        }
    }
}
