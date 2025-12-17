CREATE TABLE IF NOT EXISTS `Category` (
  `categoryId` INTEGER PRIMARY KEY,
  `name` TEXT UNIQUE NOT NULL,
  `shorthand` TEXT UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS `Subcategory` (
  `subcategoryId` INTEGER PRIMARY KEY,
  `name` TEXT UNIQUE NOT NULL,
  `shorthand` TEXT UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS `CatSubcat` (
  `catSubcatId` INTEGER PRIMARY KEY,
  `categoryId` INTEGER NOT NULL,
  `subcategoryId` INTEGER NOT NULL,
  FOREIGN KEY (`categoryId`) REFERENCES `Category` (`categoryId`),
  FOREIGN KEY (`subcategoryId`) REFERENCES `Subcategory` (`subcategoryId`)
  UNIQUE (`categoryId`, `subcategoryId`)
);

CREATE TABLE IF NOT EXISTS `Class` (
  `classId` INTEGER PRIMARY KEY,
  `name` TEXT UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS `State` (
  `stateId` INTEGER PRIMARY KEY,
  `name` TEXT NOT NULL,
  `classId` INTEGER NOT NULL,
  FOREIGN KEY (`classId`) REFERENCES `Class` (`classId`)
);

CREATE TABLE IF NOT EXISTS `Item` (
  `itemId` INTEGER PRIMARY KEY,
  `productCode` TEXT NOT NULL,
  `catSubcatId` INTEGER NOT NULL,
  `classId` INTEGER NOT NULL,
  `description` TEXT NOT NULL,
  `createdAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  `updatedAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  FOREIGN KEY (`catSubcatId`) REFERENCES `CatSubcat` (`catSubcatId`),
  FOREIGN KEY (`classId`) REFERENCES `Class` (`classId`)
);

CREATE TABLE IF NOT EXISTS `ItemStock` (
  `itemStockId` INTEGER PRIMARY KEY,
  `itemId` INTEGER UNIQUE NOT NULL,
  `additionalNotes` TEXT,
  `location` TEXT,
  `stateId` INTEGER NOT NULL,
  `createdAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  `updatedAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  FOREIGN KEY (`itemId`) REFERENCES `Item` (`itemId`),
  FOREIGN KEY (`stateId`) REFERENCES `State` (`stateId`)
);

CREATE TABLE IF NOT EXISTS `Order` (
  `orderId` INTEGER PRIMARY KEY,
  `name` TEXT,
  `description` TEXT,
  `createdAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  `updatedAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP)
);

CREATE TABLE IF NOT EXISTS `ShipmentState` (
  `shipmentStateId` INTEGER PRIMARY KEY,
  `name` TEXT UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS `OrderDetail` (
  `orderDetailId` INTEGER PRIMARY KEY,
  `orderId` INTEGER NOT NULL,
  `itemId` INTEGER NOT NULL,
  `shipmentStateId` INTEGER NOT NULL,
  `quantity` INTEGER NOT NULL DEFAULT 1,
  `createdAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  `updatedAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  FOREIGN KEY (`orderId`) REFERENCES `Order` (`orderId`),
  FOREIGN KEY (`itemId`) REFERENCES `Item` (`itemId`),
  FOREIGN KEY (`shipmentStateId`) REFERENCES `ShipmentState` (`shipmentStateId`)
);

CREATE TABLE IF NOT EXISTS `ShipmentMove` (
  `shipmentMoveId` INTEGER PRIMARY KEY,
  `orderId` INTEGER NOT NULL,
  `orderDetailId` INTEGER NOT NULL,
  `fromState` INTEGER NOT NULL DEFAULT 1,
  `toState` INTEGER NOT NULL,
  `createdAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  FOREIGN KEY (`orderId`) REFERENCES `Order` (`orderId`),
  FOREIGN KEY (`orderDetailId`) REFERENCES `OrderDetail` (`orderDetailId`),
  FOREIGN KEY (`fromState`) REFERENCES `ShipmentState` (`shipmentStateId`),
  FOREIGN KEY (`toState`) REFERENCES `ShipmentState` (`shipmentStateId`)
);