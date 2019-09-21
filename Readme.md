# Introduction

Writing a code coverage tool using .net core & where possible .net standards 2.0.

This piece of code is WIP as I want to include the instrument build up during the MSBuild process. Currently you have to build the Service project and then run the Builder > Program.cs which will rewrite the IL. Once done then you have to run the unit test. As a result of that you will find a coverage.txt file which is a plain text file with paths information based upon the unit test you see!