-- phpMyAdmin SQL Dump
-- version 5.2.3
-- https://www.phpmyadmin.net/
--
-- Host: mysql-db:3306
-- Generation Time: Nov 19, 2025 at 01:33 AM
-- Server version: 9.5.0
-- PHP Version: 8.3.26

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `runners_db`
--
CREATE DATABASE IF NOT EXISTS `runners_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `runners_db`;

-- --------------------------------------------------------

--
-- Table structure for table `categorias_evento`
--

CREATE TABLE `categorias_evento` (
  `idCategoria` int NOT NULL,
  `idEvento` int NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Ej: 10k Competitiva, 2k Corre Caminata',
  `costoInscripcion` decimal(10,2) NOT NULL DEFAULT '0.00',
  `cupoCategoria` int DEFAULT NULL,
  `edadMinima` int DEFAULT '0',
  `edadMaxima` int DEFAULT '99',
  `genero` enum('F','M','X') COLLATE utf8mb4_unicode_ci DEFAULT 'X' COMMENT 'Categoría aplica a Femenino, Masculino o Mixto/Todos (X)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `eventos`
--

CREATE TABLE `eventos` (
  `idEvento` int NOT NULL,
  `nombre` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `descripcion` text COLLATE utf8mb4_unicode_ci,
  `fechaHora` datetime NOT NULL,
  `lugar` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `cupoTotal` int DEFAULT NULL,
  `idOrganizador` int NOT NULL,
  `urlPronosticoClima` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `datosPago` text COLLATE utf8mb4_unicode_ci COMMENT 'Datos para transferencia (CBU, Alias, Titular)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `inscripciones`
--

CREATE TABLE `inscripciones` (
  `idInscripcion` int NOT NULL,
  `idUsuario` int NOT NULL,
  `idCategoria` int NOT NULL,
  `fechaInscripcion` datetime DEFAULT CURRENT_TIMESTAMP,
  `estadoPago` enum('pendiente','en_revision','pagado','rechazado') COLLATE utf8mb4_unicode_ci DEFAULT 'pendiente',
  `talleRemera` enum('XS','S','M','L','XL','XXL') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `aceptoDeslinde` tinyint(1) DEFAULT '0',
  `comprobantePagoURL` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'URL o path al comprobante subido'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `notificaciones_evento`
--

CREATE TABLE `notificaciones_evento` (
  `idNotificacion` int NOT NULL,
  `idEvento` int NOT NULL COMMENT 'Evento al que se asocia',
  `titulo` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Ej: Evento Suspendido',
  `mensaje` text COLLATE utf8mb4_unicode_ci COMMENT 'Ej: La carrera se pasa al próximo domingo...',
  `fechaEnvio` datetime DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `puntosinteres`
--

CREATE TABLE `puntosinteres` (
  `idPuntoInteres` int NOT NULL,
  `idEvento` int NOT NULL,
  `tipo` enum('hidratacion','primeros_auxilios','meta','largada','otro') COLLATE utf8mb4_unicode_ci NOT NULL,
  `nombre` varchar(100) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `resultados`
--

CREATE TABLE `resultados` (
  `idResultado` int NOT NULL,
  `idInscripcion` int NOT NULL,
  `tiempoOficial` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Ej: 00:45:30.123',
  `posicionGeneral` int DEFAULT NULL,
  `posicionCategoria` int DEFAULT NULL,
  `tiempoSmartwatch` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Ej: 00:45:28',
  `distanciaKm` decimal(6,2) DEFAULT NULL COMMENT 'Ej: 10.02',
  `ritmoPromedio` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Ej: 4:32 min/km',
  `velocidadPromedio` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Ej: 13.2 km/h',
  `caloriasQuemadas` int DEFAULT NULL,
  `pulsacionesPromedio` int DEFAULT NULL,
  `pulsacionesMax` int DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `rutas`
--

CREATE TABLE `rutas` (
  `idRuta` int NOT NULL,
  `idEvento` int NOT NULL,
  `orden` int NOT NULL COMMENT 'Orden del punto en el trazado',
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Table structure for table `usuarios`
--

CREATE TABLE `usuarios` (
  `idUsuario` int NOT NULL,
  `nombre` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL COMMENT 'Nombre (si es Runner) o Nombre de Entidad (si es Organizador)',
  `apellido` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Apellido (solo para Runners, NULL para Organizadores)',
  `email` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `telefono` int NOT NULL COMMENT 'Numero de celu',
  `passwordHash` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `tipoUsuario` enum('runner','organizador') COLLATE utf8mb4_unicode_ci NOT NULL,
  `fechaNacimiento` date DEFAULT NULL COMMENT 'Solo para Runners',
  `genero` enum('F','M','X') COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Genero del usuario (solo para Runners)',
  `dni` int DEFAULT NULL COMMENT 'DNI(solo para Runners)',
  `localidad` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Localidad de origen (para Runners)',
  `agrupacion` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Agrupación o team (ej. UTEP) (solo para Runners)',
  `telefonoEmergencia` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL COMMENT 'Contacto de emergencia (solo para Runners)',
  `estado` tinyint(1) NOT NULL DEFAULT '1' COMMENT '0 false, 1 true (Al crear) sera de estado true'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Indexes for dumped tables
--

--
-- Indexes for table `categorias_evento`
--
ALTER TABLE `categorias_evento`
  ADD PRIMARY KEY (`idCategoria`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indexes for table `eventos`
--
ALTER TABLE `eventos`
  ADD PRIMARY KEY (`idEvento`),
  ADD KEY `idOrganizador` (`idOrganizador`);

--
-- Indexes for table `inscripciones`
--
ALTER TABLE `inscripciones`
  ADD PRIMARY KEY (`idInscripcion`),
  ADD KEY `idUsuario` (`idUsuario`),
  ADD KEY `idCategoria` (`idCategoria`);

--
-- Indexes for table `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  ADD PRIMARY KEY (`idNotificacion`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indexes for table `puntosinteres`
--
ALTER TABLE `puntosinteres`
  ADD PRIMARY KEY (`idPuntoInteres`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indexes for table `resultados`
--
ALTER TABLE `resultados`
  ADD PRIMARY KEY (`idResultado`),
  ADD UNIQUE KEY `idInscripcion` (`idInscripcion`) COMMENT 'Garantiza Relación 1:1';

--
-- Indexes for table `rutas`
--
ALTER TABLE `rutas`
  ADD PRIMARY KEY (`idRuta`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indexes for table `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`idUsuario`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `email_2` (`email`),
  ADD KEY `dni` (`dni`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `categorias_evento`
--
ALTER TABLE `categorias_evento`
  MODIFY `idCategoria` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `eventos`
--
ALTER TABLE `eventos`
  MODIFY `idEvento` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `inscripciones`
--
ALTER TABLE `inscripciones`
  MODIFY `idInscripcion` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  MODIFY `idNotificacion` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `puntosinteres`
--
ALTER TABLE `puntosinteres`
  MODIFY `idPuntoInteres` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `resultados`
--
ALTER TABLE `resultados`
  MODIFY `idResultado` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `rutas`
--
ALTER TABLE `rutas`
  MODIFY `idRuta` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT for table `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `idUsuario` int NOT NULL AUTO_INCREMENT;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `categorias_evento`
--
ALTER TABLE `categorias_evento`
  ADD CONSTRAINT `categorias_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Constraints for table `eventos`
--
ALTER TABLE `eventos`
  ADD CONSTRAINT `eventos_ibfk_1` FOREIGN KEY (`idOrganizador`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE;

--
-- Constraints for table `inscripciones`
--
ALTER TABLE `inscripciones`
  ADD CONSTRAINT `inscripciones_ibfk_1` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE,
  ADD CONSTRAINT `inscripciones_ibfk_2` FOREIGN KEY (`idCategoria`) REFERENCES `categorias_evento` (`idCategoria`) ON DELETE CASCADE;

--
-- Constraints for table `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  ADD CONSTRAINT `notificaciones_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Constraints for table `puntosinteres`
--
ALTER TABLE `puntosinteres`
  ADD CONSTRAINT `puntosinteres_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Constraints for table `resultados`
--
ALTER TABLE `resultados`
  ADD CONSTRAINT `resultados_ibfk_1` FOREIGN KEY (`idInscripcion`) REFERENCES `inscripciones` (`idInscripcion`) ON DELETE CASCADE;

--
-- Constraints for table `rutas`
--
ALTER TABLE `rutas`
  ADD CONSTRAINT `rutas_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
