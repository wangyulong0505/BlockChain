﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace FiiiChain.Data {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "15.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resource {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resource() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("FiiiChain.Data.Resource", typeof(Resource).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性
        ///   重写当前线程的 CurrentUICulture 属性。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 PRAGMA foreign_keys = off;
        ///BEGIN TRANSACTION;
        ///
        ///-- 表：Accounts
        ///CREATE TABLE IF NOT EXISTS Accounts (
        ///    Id          VARCHAR (64) NOT NULL
        ///                             PRIMARY KEY,
        ///    PrivateKey  VARCHAR (64),
        ///    PublicKey   VARCHAR (64),
        ///    Balance     INTEGER      NOT NULL
        ///                             DEFAULT 0,
        ///    IsDefault   BOOLEAN      NOT NULL
        ///                             DEFAULT 0,
        ///    WatchedOnly BOOLEAN      NOT NULL
        ///                             DEFAULT 0,
        ///    Timestamp   INTEGER  [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string InitScript {
            get {
                return ResourceManager.GetString("InitScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Filename=./fiiichain.db; Mode=ReadWriteCreate;Cache=Shared; 的本地化字符串。
        /// </summary>
        internal static string MainnetConnectionString {
            get {
                return ResourceManager.GetString("MainnetConnectionString", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 PRAGMA foreign_keys = off;
        ///BEGIN TRANSACTION;
        ///
        ///CREATE TABLE sqlitestudio_temp_table1 AS SELECT *
        ///                                          FROM InputList;
        ///
        ///DROP TABLE InputList;
        ///
        ///CREATE TABLE InputList (
        ///    Id                    INTEGER      NOT NULL
        ///                                       PRIMARY KEY AUTOINCREMENT,
        ///    TransactionHash       VARCHAR (64) NOT NULL,
        ///    OutputTransactionHash VARCHAR (64) NOT NULL,
        ///    OutputIndex           INT,
        ///    Size                  INT          NOT NULL,
        ///  [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string Patch1Script {
            get {
                return ResourceManager.GetString("Patch1Script", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 UPDATE OutputList SET Spent = 1 WHERE Amount &lt;= 0 的本地化字符串。
        /// </summary>
        internal static string Patch2Script {
            get {
                return ResourceManager.GetString("Patch2Script", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 DROP INDEX IF EXISTS PrivateKey; 的本地化字符串。
        /// </summary>
        internal static string Patch3Script {
            get {
                return ResourceManager.GetString("Patch3Script", resourceCulture);
            }
        }

        /// <summary>
        ///   查找类似 delete from InputList where BlockHash in (select Hash from Blocks where Height &gt;= 101016) and not exists (select Hash from Blocks where Hash = &apos;051AEE33E5E512B6283D6F8A70517228E759A08A91C5D38B8C87CB58BC2C867F&apos; and Height = 101017);
        ///delete from OutputList where BlockHash in (select Hash from Blocks where Height &gt;= 101016) and not exists (select Hash from Blocks where Hash = &apos;051AEE33E5E512B6283D6F8A70517228E759A08A91C5D38B8C87CB58BC2C867F&apos; and Height = 101017);
        ///delete from Transactions where BlockHash in ( [字符串的其余部分被截断]&quot;; 的本地化字符串。
        /// </summary>
        internal static string Patch4Script
        {
            get
            {
                return ResourceManager.GetString("Patch4Script", resourceCulture);
            }
        }

        ///   查找类似 PRAGMA journal_mode=wal; 的本地化字符串。
        /// </summary>
        internal static string Patch5Script
        {
            get
            {
                return ResourceManager.GetString("Patch5Script", resourceCulture);
            }
        }

        /// <summary>
        ///   查找类似 Filename=./fiiichain_test.db; Mode=ReadWriteCreate;Cache=Shared; 的本地化字符串。
        /// </summary>
        internal static string TestnetConnectionString {
            get {
                return ResourceManager.GetString("TestnetConnectionString", resourceCulture);
            }
        }
    }
}
