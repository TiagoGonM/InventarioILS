-- Tablas

CREATE TABLE IF NOT EXISTS `Category` (
  `categoryId` INTEGER PRIMARY KEY,
  `name` TEXT UNIQUE NOT NULL,
  `shorthand` TEXT UNIQUE NOT NULL
);

CREATE TABLE IF NOT EXISTS `Subcategory` (
  `subcategoryId` INTEGER PRIMARY KEY,
  `name` TEXT UNIQUE NOT NULL,
  `shorthand` TEXT UNIQUE
);

CREATE TABLE IF NOT EXISTS `CatSubcat` (
  `catSubcatId` INTEGER PRIMARY KEY,
  `categoryId` INTEGER NOT NULL,
  `subcategoryId` INTEGER NOT NULL,
  FOREIGN KEY (`categoryId`) REFERENCES `Category` (`categoryId`) ON DELETE CASCADE,
  FOREIGN KEY (`subcategoryId`) REFERENCES `Subcategory` (`subcategoryId`) ON DELETE CASCADE,
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
  `isDeleted` INTEGER DEFAULT 0,
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
  `received` INTEGER NOT NULL DEFAULT 0,
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
  `fromState` INTEGER NOT NULL,
  `toState` INTEGER NOT NULL,
  `createdAt` TIMESTAMP DEFAULT (CURRENT_TIMESTAMP),
  FOREIGN KEY (`orderId`) REFERENCES `Order` (`orderId`),
  FOREIGN KEY (`orderDetailId`) REFERENCES `OrderDetail` (`orderDetailId`),
  FOREIGN KEY (`fromState`) REFERENCES `ShipmentState` (`shipmentStateId`),
  FOREIGN KEY (`toState`) REFERENCES `ShipmentState` (`shipmentStateId`)
);


-- Indices compuestos

CREATE INDEX IF NOT EXISTS idx_item_product_performance 
ON Item (productCode, isDeleted, classId);


-- Vistas

CREATE VIEW IF NOT EXISTS View_ItemStockSummary AS
  SELECT 
      sto.itemStockId id,
      it.productCode,
      c.categoryId,
      c.name category,
      s.subcategoryId,
      s.name subcategory,
      class.classId,
      CONCAT(
        UPPER(SUBSTRING(class.name, 1, 1)),
        LOWER(SUBSTRING(class.name, 2, LENGTH(class.name)))
      ) class,
      it.description, 
      st.stateId,
      CONCAT(
        UPPER(SUBSTRING(st.name, 1, 1)),
        LOWER(SUBSTRING(st.name, 2, LENGTH(st.name)))
      ) state,
      CONCAT(
        UPPER(SUBSTRING(sto.location, 1, 1)),
        LOWER(SUBSTRING(sto.location, 2, LENGTH(sto.location)))
      ) location,
      sto.additionalNotes, 
      COUNT(it.itemId) AS quantity
  FROM ItemStock sto
  JOIN Item it ON sto.itemId = it.itemId
  JOIN Class class ON it.classId = class.classId
  JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
  JOIN Category c ON cs.categoryId = c.categoryId
  JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
  JOIN State st ON sto.stateId = st.stateId
  WHERE it.isDeleted = 0
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
      c.categoryId,
      s.subcategoryId,
      it.description,
      it.classId,
      CONCAT(
        UPPER(SUBSTRING(class.name, 1, 1)),
        LOWER(SUBSTRING(class.name, 2, LENGTH(class.name)))
      ) class,
      ordDet.shipmentStateId,
      ordDet.received,
      CONCAT(
        UPPER(SUBSTRING(ss.name, 1, 1)),
        LOWER(SUBSTRING(ss.name, 2, LENGTH(ss.name)))
      ) shipmentState,
      COUNT(*) quantity
  FROM OrderDetail ordDet
  JOIN 'Order' ord ON ordDet.orderId = ord.orderId
  JOIN Item it ON ordDet.itemId = it.itemId
  JOIN ShipmentState ss ON ordDet.shipmentStateId = ss.shipmentStateId
  JOIN Class class ON it.classId = class.classId
  JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
  JOIN Category c ON cs.categoryId = c.categoryId
  JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
  GROUP BY 
      ord.orderId,
      ord.name,
      it.productCode,
      it.description,
      class.name,
      ss.name;

CREATE VIEW IF NOT EXISTS View_NoStockItems AS
SELECT 
    it.itemId id,
    it.productCode,
    c.categoryId,
    c.name category,
    s.subcategoryId,
    s.name subcategory,
    class.classId,
    CONCAT(
      UPPER(SUBSTRING(c.name, 1, 1)),
      LOWER(SUBSTRING(c.name, 2, LENGTH(c.name)))
    ) class,
    it.description,
    0 AS quantity
FROM Item it
JOIN Class class ON it.classId = class.classId
JOIN CatSubcat cs ON it.catSubcatId = cs.catSubcatId
JOIN Category c ON cs.categoryId = c.categoryId
JOIN Subcategory s ON cs.subcategoryId = s.subcategoryId
WHERE it.isDeleted = 1
  AND NOT EXISTS (
      SELECT 1
      FROM Item currentIt
      WHERE currentIt.productCode = it.productCode 
        AND currentIt.isDeleted = 0
  )
GROUP BY it.productCode;

-- Datos iniciales

-- 1. Clases
INSERT OR IGNORE INTO Class (name) VALUES
    ('insumo'),
    ('repuesto'),
    ('dispositivo');

-- 2. Categorías
-- INSERT OR IGNORE INTO Category (name) VALUES
--    ('capacitor'), ('resistor'), ('transistor'),
--    ('osciloscopio'), ('multimetro');

-- 3. Subcategorías
--INSERT OR IGNORE INTO Subcategory (name) VALUES
--    ('ceramico'), ('electrolitico'), ('estandar/no definido');

-- 4. Estados (Agrupados por clase para legibilidad)
INSERT OR IGNORE INTO State (name, classId) VALUES
    ('suficientes', 1), ('comprar', 1),                      -- Insumos
    ('disponible', 2), ('utilizado', 2), ('a descartar', 2), -- Repuestos
    ('funciona', 3), ('a reparar', 3), ('a descartar', 3);   -- Dispositivos

-- 5. Estados de Envío
INSERT OR IGNORE INTO ShipmentState (name) VALUES
    ('no realizado'), ('pedido enviado'), ('pedido notado'),
    ('en preparacion'), ('contabilizacion'), ('despachado'),
    ('hacia central de correo'), ('en central de correo'),
    ('en camino')

-- 6. Relaciones Categoría-Subcategoría
-- Nota: Asegúrate de que los IDs coincidan con el orden de inserción anterior
--INSERT OR IGNORE INTO CatSubcat (categoryId, subcategoryId) VALUES 
--    (1, 1), (1, 2), (1, 4), -- Capacitores
--    (2, 3), (2, 4),         -- Resistores
--    (5, 4);                 -- Multímetros

--INSERT OR IGNORE INTO CatSubcat (categoryId, subcategoryId)
--SELECT c.categoryId, s.subcategoryId
--FROM Category c, Subcategory s
--WHERE (c.name = 'capacitor' AND s.name = 'cerámico')
--   OR (c.name = 'capacitor' AND s.name = 'electrolítico')
--   OR (c.name = 'resistor'  AND s.name = 'estándar')
--   OR (c.name = 'multimetro' AND s.name = 'estandar');