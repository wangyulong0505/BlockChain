﻿CREATE TABLE [dbo].[PaymentRequests]
(
	[Id] BIGINT NOT NULL PRIMARY KEY IDENTITY, 
    [AccountId] VARCHAR(64) NOT NULL, 
    [Tag] VARCHAR(64) NULL, 
    [Comment] VARCHAR(200) NULL, 
    [Amount] BIGINT NOT NULL DEFAULT 0, 
    [Timestamp] BIGINT NOT NULL DEFAULT 0
)
