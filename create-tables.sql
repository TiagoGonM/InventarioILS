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

CREATE VIEW IF NOT EXISTS View_ItemStockSummary AS
  SELECT 
      sto.itemStockId AS id, 
      it.productCode, 
      c.name AS category, 
      s.name AS subcategory,
      CONCAT(
        UPPER(SUBSTRING(class.name, 1, 1)),
        LOWER(SUBSTRING(class.name, 2, LENGTH(class.name)))
    ) class, 
      it.description, 
      CONCAT(
        UPPER(SUBSTRING(st.name, 1, 1)),
        LOWER(SUBSTRING(st.name, 2, LENGTH(st.name)))
    ) state,
    CONCAT(
        UPPER(SUBSTRING(sto.location, 1, 1)),
        LOWER(SUBSTRING(sto.location, 2, LENGTH(sto.location)))
    ) location, 
      sto.additionalNotes, 
      COUNT(*) AS quantity
  FROM ItemStock sto
  JOIN Item it ON sto.itemId = it.itemId
  JOIN Class class ON it.classId = class.classId
  JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
  JOIN Category c ON cs.categoryId = c.categoryId
  JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
  JOIN State st ON sto.stateId = st.stateId
  GROUP BY 
      it.productCode, 
      c.name, 
      s.name, 
      class.name,
      st.name,
      sto.location;

CREATE VIEW IF NOT EXISTS View_OrderItemsSummary AS
  SELECT 
      ord.orderId,                     
      ord.name orderName, 
      it.productCode, 
      it.description,
      CONCAT(
        UPPER(SUBSTRING(c.name, 1, 1)),
        LOWER(SUBSTRING(c.name, 2, LENGTH(c.name)))
      ) class,  
      CONCAT(
        UPPER(SUBSTRING(ss.name, 1, 1)),
        LOWER(SUBSTRING(ss.name, 2, LENGTH(ss.name)))
      ) shipmentState,       
      COUNT(*) quantity
  FROM OrderDetail ordDet
  JOIN 'Order' ord ON ordDet.orderId = ord.orderId
  JOIN Item it ON ordDet.itemId = it.itemId
  JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
  JOIN Class c ON it.classId = c.classId
  GROUP BY 
      ord.orderId, 
      ord.name, 
      it.productCode, 
      it.description, 
      c.name, 
      ss.name;