-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Servidor: 127.0.0.1
-- Tiempo de generación: 13-11-2025 a las 04:00:00
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
-- Estructura de tabla para la tabla `usuarios`
-- (Tabla 1 de 8)
-- Almacena ambos roles: "runner" (persona) y "organizador" (entidad).
--

CREATE TABLE `usuarios` (
  `idUsuario` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL COMMENT 'Nombre (si es Runner) o Nombre de Entidad (si es Organizador)',
  `apellido` varchar(100) DEFAULT NULL COMMENT 'Apellido (solo para Runners, NULL para Organizadores)',
  `email` varchar(100) NOT NULL,
  `passwordHash` varchar(255) NOT NULL,
  `tipoUsuario` enum('runner','organizador') NOT NULL,
  `fechaNacimiento` date DEFAULT NULL COMMENT 'Solo para Runners',
  -- Campos v2 (del formulario de inscripción)
  `genero` enum('F','M','X') DEFAULT NULL COMMENT 'Genero del usuario (solo para Runners)',
  `dni` varchar(20) DEFAULT NULL COMMENT 'Documento Nacional de Identidad (solo para Runners)',
  `localidad` varchar(100) DEFAULT NULL COMMENT 'Localidad de origen (para Runners)',
  `agrupacion` varchar(100) DEFAULT NULL COMMENT 'Agrupación o team (ej. UTEP) (solo para Runners)',
  `telefonoEmergencia` varchar(50) DEFAULT NULL COMMENT 'Contacto de emergencia (solo para Runners)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `eventos`
-- (Tabla 2 de 8)
-- Almacena la información general de la carrera.
--

CREATE TABLE `eventos` (
  `idEvento` int(11) NOT NULL,
  `nombre` varchar(255) NOT NULL,
  `descripcion` text DEFAULT NULL,
  `fechaHora` datetime NOT NULL,
  `lugar` varchar(255) DEFAULT NULL,
  `cupoTotal` int(11) DEFAULT NULL,
  `idOrganizador` int(11) NOT NULL,
  `urlPronosticoClima` varchar(255) DEFAULT NULL,
  -- Campo v2 (para pagos manuales)
  `datosPago` text DEFAULT NULL COMMENT 'Datos para transferencia (CBU, Alias, Titular)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `categorias_evento`
-- (Tabla 3 de 8)
-- Define las diferentes distancias y categorías (ej. 10k, 21k) de un evento.
--

CREATE TABLE `categorias_evento` (
  `idCategoria` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `nombre` varchar(100) NOT NULL COMMENT 'Ej: 10k Competitiva, 2k Corre Caminata',
  `costoInscripcion` decimal(10,2) DEFAULT 0.00,
  `cupoCategoria` int(11) DEFAULT NULL,
  -- Campos v2 (para lógica de categorías de sanluisrun.com.ar)
  `edadMinima` int(3) DEFAULT 0,
  `edadMaxima` int(3) DEFAULT 99,
  `genero` enum('F','M','X') DEFAULT 'X' COMMENT 'Categoría aplica a Femenino, Masculino o Mixto/Todos (X)'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `inscripciones`
-- (Tabla 4 de 8)
-- Tabla intermedia clave. Conecta un Usuario con una Categoría de Evento.
--

CREATE TABLE `inscripciones` (
  `idInscripcion` int(11) NOT NULL,
  `idUsuario` int(11) NOT NULL,
  `idCategoria` int(11) NOT NULL,
  `fechaInscripcion` datetime DEFAULT current_timestamp(),
  -- Campos v2 y v3 (logística y pago)
  `estadoPago` enum('pendiente','en_revision','pagado','rechazado') DEFAULT 'pendiente',
  `talleRemera` enum('XS','S','M','L','XL','XXL') DEFAULT NULL,
  `aceptoDeslinde` tinyint(1) DEFAULT 0,
  `comprobantePagoURL` varchar(255) DEFAULT NULL COMMENT 'URL o path al comprobante subido'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `resultados`
-- (Tabla 5 de 8)
-- Almacena los resultados. Relación 1:1 con Inscripciones.
--

CREATE TABLE `resultados` (
  `idResultado` int(11) NOT NULL,
  `idInscripcion` int(11) NOT NULL,
  -- Campos v1 (Cargados por el Organizador, manual o CSV)
  `tiempoOficial` varchar(20) DEFAULT NULL COMMENT 'Ej: 00:45:30.123',
  `posicionGeneral` int(11) DEFAULT NULL,
  `posicionCategoria` int(11) DEFAULT NULL,
  -- Campos v3 (Anexados por el Runner, manual o Google Fit)
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
-- (Tabla 6 de 8)
-- Almacena los puntos (coordenadas) para dibujar el mapa del recorrido.
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
-- Estructura de tabla para la tabla `puntosinteres`
-- (Tabla 7 de 8)
-- Almacena los Puntos de Interés (hidratación, meta, etc.) del evento.
--

CREATE TABLE `puntosinteres` (
  `idPuntoInteres` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL,
  `tipo` enum('hidratacion','primeros_auxilios','meta','largada','otro') NOT NULL,
  `nombre` varchar(100) DEFAULT NULL,
  `latitud` decimal(10,7) NOT NULL,
  `longitud` decimal(10,7) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- --------------------------------------------------------

--
-- Estructura de tabla para la tabla `notificaciones_evento`
-- (Tabla 8 de 8) - Añadida en v3
-- Almacena el "Buzón de Notificaciones" PULL.
--

CREATE TABLE `notificaciones_evento` (
  `idNotificacion` int(11) NOT NULL,
  `idEvento` int(11) NOT NULL COMMENT 'Evento al que se asocia',
  `titulo` varchar(255) NOT NULL COMMENT 'Ej: Evento Suspendido',
  `mensaje` text DEFAULT NULL COMMENT 'Ej: La carrera se pasa al próximo domingo...',
  `fechaEnvio` datetime DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Índices para tablas volcadas
--

ALTER TABLE `usuarios`
  ADD PRIMARY KEY (`idUsuario`),
  ADD UNIQUE KEY `email` (`email`),
  ADD KEY `email_2` (`email`),
  ADD KEY `dni` (`dni`);

ALTER TABLE `eventos`
  ADD PRIMARY KEY (`idEvento`),
  ADD KEY `idOrganizador` (`idOrganizador`);

ALTER TABLE `categorias_evento`
  ADD PRIMARY KEY (`idCategoria`),
  ADD KEY `idEvento` (`idEvento`);

ALTER TABLE `inscripciones`
  ADD PRIMARY KEY (`idInscripcion`),
  ADD KEY `idUsuario` (`idUsuario`),
  ADD KEY `idCategoria` (`idCategoria`);

ALTER TABLE `resultados`
  ADD PRIMARY KEY (`idResultado`),
  ADD UNIQUE KEY `idInscripcion` (`idInscripcion`) COMMENT 'Garantiza Relación 1:1';

ALTER TABLE `rutas`
  ADD PRIMARY KEY (`idRuta`),
  ADD KEY `idEvento` (`idEvento`);

ALTER TABLE `puntosinteres`
  ADD PRIMARY KEY (`idPuntoInteres`),
  ADD KEY `idEvento` (`idEvento`);

ALTER TABLE `notificaciones_evento`
  ADD PRIMARY KEY (`idNotificacion`),
  ADD KEY `idEvento` (`idEvento`);

--
-- AUTO_INCREMENT de las tablas volcadas
--

ALTER TABLE `usuarios`
  MODIFY `idUsuario` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `eventos`
  MODIFY `idEvento` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `categorias_evento`
  MODIFY `idCategoria` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `inscripciones`
  MODIFY `idInscripcion` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `resultados`
  MODIFY `idResultado` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `rutas`
  MODIFY `idRuta` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `puntosinteres`
  MODIFY `idPuntoInteres` int(11) NOT NULL AUTO_INCREMENT;

ALTER TABLE `notificaciones_evento`
  MODIFY `idNotificacion` int(11) NOT NULL AUTO_INCREMENT;

--
-- Restricciones para tablas volcadas
--

ALTER TABLE `eventos`
  ADD CONSTRAINT `eventos_ibfk_1` FOREIGN KEY (`idOrganizador`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE;

ALTER TABLE `categorias_evento`
  ADD CONSTRAINT `categorias_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

ALTER TABLE `inscripciones`
  ADD CONSTRAINT `inscripciones_ibfk_1` FOREIGN KEY (`idUsuario`) REFERENCES `usuarios` (`idUsuario`) ON DELETE CASCADE,
  ADD CONSTRAINT `inscripciones_ibfk_2` FOREIGN KEY (`idCategoria`) REFERENCES `categorias_evento` (`idCategoria`) ON DELETE CASCADE;

ALTER TABLE `resultados`
  ADD CONSTRAINT `resultados_ibfk_1` FOREIGN KEY (`idInscripcion`) REFERENCES `inscripciones` (`idInscripcion`) ON DELETE CASCADE;

ALTER TABLE `rutas`
  ADD CONSTRAINT `rutas_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

ALTER TABLE `puntosinteres`
  ADD CONSTRAINT `puntosinteres_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;
  
ALTER TABLE `notificaciones_evento`
  ADD CONSTRAINT `notificaciones_evento_ibfk_1` FOREIGN KEY (`idEvento`) REFERENCES `eventos` (`idEvento`) ON DELETE CASCADE;

COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;