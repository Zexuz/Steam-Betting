namespace Betting.Repository.Helpers
{
    public static class SqlQueries
    {
        public static string CreateMainDatabaseTablesQuery =

            #region query

            @"
								CREATE TABLE @databaseName.dbo.ItemDescription
								(
								  Id        INT            IDENTITY PRIMARY KEY,
								  Name      VARCHAR(140)   NOT NULL,
								  Value     DECIMAL(10, 2) NOT NULL,
								  ImageUrl  VARCHAR(250)   NOT NULL,

								  AppId     VARCHAR(10)    NOT NULL,
								  ContextId VARCHAR(10)    NOT NULL,
								  Valid 	BIT			   NOT NULL,
								)
								
								
								CREATE UNIQUE INDEX ItemDescription_Name_uindex
								  ON @databaseName.dbo.ItemDescription (Name)
								
								
								CREATE TABLE @databaseName.dbo.[User]
								(
								  Id                 INT                        IDENTITY PRIMARY KEY,
								  SteamId            VARCHAR(20)                NOT NULL,
								  Name               VARCHAR(50)                NOT NULL,
								  ImageUrl           VARCHAR(100)               NOT NULL,
								  TradeLink          VARCHAR(100),
								  Quote              VARCHAR(80),
								  Created            DATETIME                   NOT NULL,
								  LastActive         DATETIME                   NOT NULL,
								  SuspendedFromQuote BIT                        NOT NULL
								)
								
								
								CREATE UNIQUE INDEX User_SteamId_uindex ON @databaseName.dbo.[User] (SteamId)

								CREATE TABLE @databaseName.dbo.GameMode
								(
									Id               INT         NOT NULL PRIMARY KEY IDENTITY,
									Type             VARCHAR(20) NOT NULL,
									CurrentSettingId INT         NOT NULL,
									IsEnabled        BIT         NOT NULL
								)
								CREATE UNIQUE INDEX GameMode_Type_uindex ON @databaseName.dbo.GameMode (Type)

								CREATE TABLE @databaseName.dbo.Match
								(
								  Id           INT IDENTITY PRIMARY KEY,
								  RoundId      INT                        NOT NULL,
								  Salt         VARCHAR(44)                NOT NULL,
								  Hash         VARCHAR(128)               NOT NULL,
								  Percentage   VARCHAR(30)				  NOT NULL,
								  Created      DATETIME                   NOT NULL,
								  TimerStarted DATETIME 					  NULL,
								  WinnerId     INT 					          NULL,
								  SettingId    INT 					      NOT NULL,
								  GameModeId   INT					      NOT NULL,
								  Status       INT                        NOT NULL,

								  CONSTRAINT Match_User_Id_fk            FOREIGN KEY (WinnerId)  REFERENCES @databaseName.dbo.[User] (Id),
								  CONSTRAINT Match_GameMode_Id_fk        FOREIGN KEY (GameModeId)  REFERENCES @databaseName.dbo.GameMode (Id)
								)								
								CREATE UNIQUE INDEX Match_RoundId_uindex ON @databaseName.dbo.Match (RoundId)
								
								CREATE TABLE @databaseName.dbo.Bot
								(
								  Id        INT          IDENTITY PRIMARY KEY,
								  SteamId   VARCHAR(20)  NOT NULL,
								  Name      VARCHAR(50)  NOT NULL
								)
								
								
								CREATE UNIQUE INDEX Bot_SteamId_uindex ON @databaseName.dbo.Bot (SteamId)
								
								
								CREATE TABLE @databaseName.dbo.Item
								(
								  Id            INT IDENTITY
									PRIMARY KEY,
								  AssetId       VARCHAR(25) NOT NULL,
								  DescriptionId INT         NOT NULL
									CONSTRAINT Item_ItemDescription_fk
									REFERENCES @databaseName.dbo.ItemDescription,
								  LocationId    INT
									CONSTRAINT Item_Location_fk
									REFERENCES @databaseName.dbo.Bot,
								  OwnerId       INT
									CONSTRAINT Item_User_fk
									REFERENCES @databaseName.dbo.[User],
								  ReleaseTime DATETIMEOFFSET DEFAULT SYSDATETIMEOFFSET() NOT NULL
								)
								CREATE UNIQUE INDEX Item_AssetId_DescriptionId_uindex ON @databaseName.dbo.Item (AssetId, DescriptionId)


								CREATE TABLE @databaseName.dbo.RakeItem
								(
								  Id            INT IDENTITY PRIMARY KEY,
								  AssetId       VARCHAR(25) NOT NULL,
								  DescriptionId INT         NOT NULL CONSTRAINT RakeItem_ItemDescription_fk REFERENCES @databaseName.dbo.ItemDescription,
								  LocationId    INT         NOT NULL CONSTRAINT RakeItem_Location_fk REFERENCES @databaseName.dbo.Bot,
								  GameModeId    INT			NOT NULL CONSTRAINT RakeItem_GameMode_fk REFERENCES @databaseName.dbo.GameMode,
								  MatchId       INT         NOT NULL,
								  Received      DATETIME    NOT NULL,
								  IsSold        BIT         NOT NULL,
								)
								
								CREATE UNIQUE INDEX RakeItem_AssetId_DescriptionId_uindex ON @databaseName.dbo.RakeItem (AssetId, DescriptionId)
								
								
								CREATE TABLE @databaseName.dbo.Bet
								(
								  Id          INT           NOT NULL IDENTITY PRIMARY KEY,
								  UserId      INT           NOT NULL CONSTRAINT User_fk REFERENCES @databaseName.dbo.[User],
								  GameModeId  INT			NOT NULL CONSTRAINT Bet_GameMode_fk REFERENCES @databaseName.dbo.GameMode,
								  MatchId     INT           NOT NULL,
                                  Created     DATETIME      NOT NULL,
								)

								CREATE UNIQUE INDEX Bet_Match_GameMode_User_uindex ON @databaseName.dbo.Bet (MatchId, GameModeId, UserId)
								
								
								CREATE TABLE @databaseName.dbo.ItemBetted
								(
									Id INT PRIMARY KEY NOT NULL IDENTITY,
									BetId INT NOT NULL,
									DescriptionId INT NOT NULL,
									AssetId VARCHAR(25) NOT NULL,
									Value DECIMAL(10,2) NOT NULL,
									CONSTRAINT ItemBetted_ItemDescription_Id_fk FOREIGN KEY (DescriptionId) REFERENCES ItemDescription (Id),
									CONSTRAINT ItemBetted_Bet_Id_fk FOREIGN KEY (BetId) REFERENCES Bet (Id)
								)
								CREATE UNIQUE INDEX ItemBetted_Id_uindex ON @databaseName.dbo.ItemBetted (Id)
								CREATE UNIQUE INDEX ItemBetted_AssetId_DescriptionId_BetId_uindex ON @databaseName.dbo.ItemBetted (BetId,AssetId, DescriptionId)

								CREATE TABLE @databaseName.dbo.OfferTransaction
								(
									Id INT PRIMARY KEY NOT NULL IDENTITY,
									UserId INT NOT NULL,
									BotId INT NOT NULL,
									TotalValue DECIMAL(12,2) NOT NULL,
									isDeposit BIT NOT NULL,
									SteamOfferId VARCHAR(50),
									Accepted DATETIME,
									CONSTRAINT Offer_Transaction__User_fk FOREIGN KEY (UserId) REFERENCES [User] (Id),
									CONSTRAINT Offer_Transaction__Bot_fk FOREIGN KEY (BotId) REFERENCES Bot (Id)
								)
								CREATE INDEX OfferTransaction_User_index ON @databaseName.dbo.OfferTransaction (UserId)
								CREATE UNIQUE INDEX OfferTransaction_Id_uindex ON @databaseName.dbo.OfferTransaction (Id)
								CREATE UNIQUE INDEX OfferTransaction_SteamOfferId_uindex ON @databaseName.dbo.OfferTransaction (SteamOfferId)
									WHERE SteamOfferId IS NOT NULL
								
								CREATE TABLE @databaseName.dbo.ItemInOfferTransaction
								(
									Id INT PRIMARY KEY NOT NULL IDENTITY,
									OfferTransactionId INT NOT NULL,
									ItemDescriptionId INT NOT NULL,
									AssetId VARCHAR(25) NOT NULL,
									Value DECIMAL(12,2) NOT NULL,
									CONSTRAINT ItemInOfferTransaction_OfferTransaction_Id_fk FOREIGN KEY (OfferTransactionId) REFERENCES OfferTransaction (Id),
									CONSTRAINT ItemInOfferTransaction_ItemDescription_Id_fk FOREIGN KEY (ItemDescriptionId) REFERENCES ItemDescription (Id)
								)
								CREATE UNIQUE INDEX ItemInOfferTransaction_AssetId_ItemDescriptionId_OfferTransactionId_uindex 
									ON @databaseName.dbo.ItemInOfferTransaction (AssetId, ItemDescriptionId, OfferTransactionId)
								CREATE UNIQUE INDEX ItemInOfferTransaction_Id_uindex ON @databaseName.dbo.ItemInOfferTransaction (Id)


								CREATE TABLE @databaseName.dbo.CoinFlip
								(
								  Id            INT IDENTITY PRIMARY KEY,
								  RoundId       VARCHAR(36)  NOT NULL,
								  Salt          VARCHAR(44)  NOT NULL,
								  Hash          VARCHAR(128) NOT NULL,
								  Percentage    VARCHAR(30)  NOT NULL,
								  Status        INT          NOT NULL,
								  WinnerId      INT              NULL   CONSTRAINT CoinFlip_User_Id_fk REFERENCES @databaseName.dbo.[User],
								  CreatorUserId INT          NOT NULL   CONSTRAINT CoinFlip_User_Creator_Id_fk REFERENCES @databaseName.dbo.[User],
								  CreatorIsHead BIT          NOT NULL,
								  TimerStarted  DATETIME         NULL,
								  SettingId     INT          NOT NULL,
								  GameModeId    INT          NOT NULL   CONSTRAINT CoinFlip_GameMode_Id_fk REFERENCES @databaseName.dbo.GameMode,
								  Created       DATETIME     NOT NULL
								)
								CREATE UNIQUE INDEX CoinFlip_RoundId_uindex ON @databaseName.dbo.CoinFlip(RoundId)
								

								CREATE TABLE @databaseName.dbo.JackpotSetting
								(
								  Id                     INT           IDENTITY PRIMARY KEY NOT NULL,
								  Rake                   DECIMAL(4, 2) NOT NULL,
								  TimmerInMilliSec       INT           NOT NULL,
								  ItemsLimit             INT           NOT NULL,
								  MaxItemAUserCanBet     INT           NOT NULL,
								  MinItemAUserCanBet     INT           NOT NULL,
								  MaxValueAUserCanBet    DECIMAL(6, 2) NOT NULL,
								  MinValueAUserCanBet    DECIMAL(6, 2) NOT NULL,
								  AllowCsgo				 BIT		   NOT NULL,
								  AllowPubg				 BIT		   NOT NULL,
								  DraftingTimeInMilliSec INT           NOT NULL,
								  DraftingGraph          VARCHAR(50)   NOT NULL
								)

";

        #endregion

        public static string CreateSettingsDatabaseTablesQuery =

            #region Query

            @"

								CREATE TABLE  @databaseName.dbo.Settings
								(
								  Id                            INT IDENTITY PRIMARY KEY,
								  InventoryLimit                INT            NOT NULL,
								  ItemValueLimit                DECIMAL(10, 2) NOT NULL,
								  SteamInventoryCacheTimerInSec INT            NOT NULL,
								  UpdatedPricingTime            DATETIME       NOT NULL,
								  NrOfLatestChatMessages        INT            NOT NULL
								)
								
								
								CREATE UNIQUE INDEX Settings_id_uindex ON  @databaseName.dbo.Settings (Id)

								CREATE TABLE @databaseName.dbo.Level
								(
									Id           INT            IDENTITY PRIMARY KEY ,
									Name         VARCHAR(50)    NOT NULL,
									Chat         BIT            NOT NULL,
									Ticket       BIT            NOT NULL,
									Admin        BIT            NOT NULL
								)
								CREATE UNIQUE INDEX Level_Name_uindex ON @databaseName.dbo.Level (Name)

								CREATE TABLE @databaseName.dbo.Staff
								(
									Id      	INT         IDENTITY PRIMARY KEY,
									SteamId 	VARCHAR(25) NOT NULL,
									Level   	INT         NOT NULL CONSTRAINT Staff_Level_Id_fk REFERENCES  @databaseName.dbo.Level
								)
								CREATE UNIQUE INDEX Staff_SteamId_uindex ON @databaseName.dbo.Staff (SteamId)

";

        #endregion
    }
}