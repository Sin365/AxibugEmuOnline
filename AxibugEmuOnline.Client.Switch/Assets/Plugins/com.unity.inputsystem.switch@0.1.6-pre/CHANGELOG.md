# Changelog
All notable changes to the input system package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

Due to package verification, the latest version below is the unpublished version and the date is meaningless.
however, it has to be formatted properly to pass verification tests.

## [0.1.0-preview] - 2019-9-2

First release from develop branch.

## [0.1.1-preview] - 2019-10-29

Updated package dependencies to use latest `com.unity.inputsystem` package version 1.0.0-preview.1

## [0.1.2-preview] - 2021-05-17

Updated package dependencies to `com.unity.inputsystem` package version 1.1.0-preview.2 and Unity 2019.4

Overrides added for aButton, bButton, xButton, yButton functions to map to north, south, east, west buttons correctly for Nintendo layout.
Fixed incorrect shortDisplayName values inherited from Gamepad.cs
Added "Submit" & "Cancel" UI usage tags to A and B buttons.
Added Joy-Con attrbutes to NPad.cs as a bit field, with accessors to get connected and wired states.
Minium Unity versions for checking m_Attributes functions (IsWired/IsConnected)
2021.2.0a10, 2021.1.1f1, 2020.3.4f1, 2019.4.24f1
Added code to parse the Unity application version string and test for NPad attributes support (Assert if attempting to use attributes and Unity version is below minimum required).

## [0.1.3-pre] - 2021-07-06

Updated package dependencies to use `com.unity.inputsystem` package 1.1.0-pre.5
This fixes an issue where earlier 'com.unity.inputsystem' packages (1.1.0-preview.X) were considered more recent than 1.1.0-pre.5 due to the Semantic Versioning spec and that prevented upgrading to 1.1.0-pre.5 or later.
Updated the package name to the use the "Pre-release" label following the new package lifecycle naming convention & patch version increased.

## [0.1.4-pre] - 2021-11-03

Fixed logic error in minimum version check (IsVersionOrHigher) for checking support for m_Attributes.
The Unity version type (alpha, beta, final etc) should be checked ahead of the revision, so 2021.2.0f1 (final 1) is recognised as a later version than 2021.2.0a10 (alpha 10)

## [0.1.5-pre] - 2021-11-15

The package is now signed.

## [0.1.6-pre] - 2022-03-55

Fixed a bug where ResetHaptics and PauseHaptics functions were not specifying a RumblePosition flag (Was None, should be All) and so had no effect when called.
NPadDeviceIOCTLOutputCommand Create function now takes RumblePosition as an argument (defaulting to All)
Fixed a bug in NPadRumbleValues.Reset() where frequencyHigh was not being reset.
