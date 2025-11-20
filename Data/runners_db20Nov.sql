-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 20-11-2025 a las 04:18:11
-- Versión del servidor: 10.4.32-MariaDB
-- Versión de PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Base de datos: `runners_db`
--
CREATE DATABASE IF NOT EXISTS `runners_db` DEFAULT CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE `runners_db`;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `categorias_evento`
--

CREATE TABLE `categorias_evento` (
  `idCategoria` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL COMMENT 'Ej: 10k Competitiva, 2k Corre Caminata',
  `costoInscripcion` decimal(10,2) NOT NULL DEFAULT 0.00,
  `cupoCategoria` int(11) DEFAULT NULL,
  `edadMinima` int(11) DEFAULT 0,
  `edadMaxima` int(11) DEFAULT 99,
  `genero` enum('F','M','X') DEFAULT 'X' COMMENT 'Categoría aplica a Femenino, Masculino o Mixto/Todos (X)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `eventos`
--

CREATE TABLE `eventos` (
  `idEvento` int(11) NOT NULL,
  `nombre` varchar(255) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `fechaHora` datetime NOT NULL,
  `lugar` varchar(255) NOT NULL,
  `cupoTotal` int(11) DEFAULT NULL,
  `idOrganizador` int(11) NOT NULL,
  `urlPronosticoClima` varchar(255) DEFAULT NULL,
  `datosPago` text DEFAULT NULL COMMENT 'Datos para transferencia (CBU, Alias, Titular), inicialmente puede ser nulo hasta que el orga cargue datos de alias',
  `estado` enum('publicado','cancelado','finalizado') NOT NULL DEFAULT 'publicado'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inscripciones`
--

CREATE TABLE `inscripciones` (
  `idInscripcion` int(11) NOT NULL,
  `idUsuario` int(11) NOT NULL,
  `idCategoria` int(11) NOT NULL,
  `fechaInscripcion` datetime DEFAULT current_timestamp(),
  `estadoPago` enum('pendiente','en_revision','pagado','rechazado') DEFAULT 'pendiente',
  `talleRemera` enum('XS','S','M','L','XL','XXL') DEFAULT NULL,
  `aceptoDeslinde` tinyint(1) NOT NULL DEFAULT 0 COMMENT 'Debe ser true (1) para aceptar el deslinde',
  `comprobantePagoURL` varchar(255) DEFAULT NULL COMMENT 'URL o path al comprobante subido'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `notificaciones_evento`
--

CREATE TABLE `notificaciones_evento` (
  `idNotificacion` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL COMMENT 'Evento al que se asocia',
  `titulo` varchar(255) NOT NULL COMMENT 'Ej: Evento Suspendido',
  `mensaje` text DEFAULT NULL COMMENT 'Ej: La carrera se pasa al próximo domingo...',
  `fechaEnvio` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `puntosinteres`
--

CREATE TABLE `puntosinteres` (
  `idPuntoInteres` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `tipo` enum('hidratacion','primeros_auxilios','meta','largada','otro') NOT NULL,
  `nombre` varchar(100) NOT NULL,
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `resultados`
--

CREATE TABLE `resultados` (
  `idResultado` int(11) NOT NULL,
  `idInscripcion` int(11) NOT NULL,
  `tiempoOficial` varchar(20) DEFAULT NULL COMMENT 'Ej: 00:45:30.123',
  `posicionGeneral` int(11) DEFAULT NULL,
  `posicionCategoria` int(11) DEFAULT NULL,
  `tiempoSmartwatch` varchar(20) DEFAULT NULL COMMENT 'Ej: 00:45:28',
  `distanciaKm` decimal(6,2) DEFAULT NULL COMMENT 'Ej: 10.02',
  `ritmoPromedio` varchar(20) DEFAULT NULL COMMENT 'Ej: 4:32 min/km',
  `velocidadPromedio` varchar(20) DEFAULT NULL COMMENT 'Ej: 13.2 km/h',
  `caloriasQuemadas` int(11) DEFAULT NULL,
  `pulsacionesPromedio` int(11) DEFAULT NULL,
  `pulsacionesMax` int(11) DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `rutas`
--

CREATE TABLE `rutas` (
  `idRuta` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `orden` int(11) NOT NULL COMMENT 'Orden del punto en el trazado',
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `usuarios`
--

CREATE TABLE `usuarios` (
  `idUsuario` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL COMMENT 'Nombre (si es Runner) o Nombre de Entidad (si es Organizador)',
  `apellido` varchar(100) DEFAULT NULL COMMENT 'Apellido (solo para Runners, NULL para Organizadores)',
  `email` varchar(100) NOT NULL,
  `telefono` int(11) NOT NULL COMMENT 'Numero de celu',
  `passwordHash` varchar(255) NOT NULL,
  `tipoUsuario` enum('runner','organizador') NOT NULL,
  `fechaNacimiento` date DEFAULT NULL COMMENT 'Solo para Runners',
  `genero` enum('F','M','X') DEFAULT NULL COMMENT 'Genero del usuario (solo para Runners)',
  `dni` int(11) DEFAULT NULL COMMENT 'DNI(solo para Runners)',
  `localidad` varchar(100) DEFAULT NULL COMMENT 'Localidad de origen (para Runners)',
  `agrupacion` varchar(100) DEFAULT NULL COMMENT 'Agrupación o team (ej. UTEP) (solo para Runners)',
  `telefonoEmergencia` varchar(50) DEFAULT NULL COMMENT 'Contacto de emergencia (solo para Runners)',
  `estado` tinyint(1) NOT NULL DEFAULT 1 COMMENT '0 false, 1 true (Al crear) sera de estado true'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Índices para tablas volcadas
--

--
-- Indices de la tabla `categorias_evento`
--
ALTER TABLE `categorias_evento`
  ADD PRIMARY KEY (`idCategoria`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `eventos`
--
ALTER TABLE `eventos`
  ADD PRIMARY KEY (`idEvento`),
  ADD KEY `idOrganizador` (`idOrganizador`);

--
-- Indices de la tabla `inscripciones`
--
ALTER TABLE `inscripciones`
  ADD PRIMARY KEY (`idInscripcion`),
  ADD KEY `idUsuario` (`idUsuario`),
  ADD KEY `idCategoria` (`idCategoria`);

--
-- Indices de la tabla `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  ADD PRIMARY KEY (`idNotificacion`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `puntosinteres`
--
ALTER TABLE `puntosinteres`
  ADD PRIMARY KEY (`idPuntoInteres`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `resultados`
--
ALTER TABLE `resultados`
  ADD PRIMARY KEY (`idResultado`),
  ADD UNIQUE KEY `idInscripcion` (`idInscripcion`) COMMENT 'Garantiza Relación 1:1';

--
-- Indices de la tabla `rutas`
--
ALTER TABLE `rutas`
  ADD PRIMARY KEY (`idRuta`),
  ADD KEY `idEvento` (`idEvento`);

--
-- Indices de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`idUsuario`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `email_2` (`email`),
  ADD KEY `dni` (`dni`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

--
-- AUTO_INCREMENT de la tabla `categorias_evento`
--
ALTER TABLE `categorias_evento`
  MODIFY `idCategoria` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `eventos`
--
ALTER TABLE `eventos`
  MODIFY `idEvento` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `inscripciones`
--
ALTER TABLE `inscripciones`
  MODIFY `idInscripcion` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  MODIFY `idNotificacion` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `puntosinteres`
--
ALTER TABLE `puntosinteres`
  MODIFY `idPuntoInteres` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `resultados`
--
ALTER TABLE `resultados`
  MODIFY `idResultado` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `rutas`
--
ALTER TABLE `rutas`
  MODIFY `idRuta` int(11) NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT de la tabla `usuarios`
--
ALTER TABLE `usuarios`
  MODIFY `idUsuario` int(11) NOT NULL AUTO_INCREMENT;

--
-- Restricciones para tablas volcadas
--

--
-- Filtros para la tabla `categorias_evento`
--
ALTER TABLE `categorias_evento`
  ADD CONSTRAINT `categorias_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Filtros para la tabla `eventos`
--
ALTER TABLE `eventos`
  ADD CONSTRAINT `eventos_ibfk_1` FOREIGN KEY (`idOrganizador`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE;

--
-- Filtros para la tabla `inscripciones`
--
ALTER TABLE `inscripciones`
  ADD CONSTRAINT `inscripciones_ibfk_1` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE,
  ADD CONSTRAINT `inscripciones_ibfk_2` FOREIGN KEY (`idCategoria`) REFERENCES `categorias_evento` (`idCategoria`) ON DELETE CASCADE;

--
-- Filtros para la tabla `notificaciones_evento`
--
ALTER TABLE `notificaciones_evento`
  ADD CONSTRAINT `notificaciones_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Filtros para la tabla `puntosinteres`
--
ALTER TABLE `puntosinteres`
  ADD CONSTRAINT `puntosinteres_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

--
-- Filtros para la tabla `resultados`
--
ALTER TABLE `resultados`
  ADD CONSTRAINT `resultados_ibfk_1` FOREIGN KEY (`idInscripcion`) REFERENCES `inscripciones` (`idInscripcion`) ON DELETE CASCADE;

--
-- Filtros para la tabla `rutas`
--
ALTER TABLE `rutas`
  ADD CONSTRAINT `rutas_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
