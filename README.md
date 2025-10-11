# Minecraft Clone - C# & OpenGL
![Project GIF](https://github.com/nioark/SharpCraft/blob/main/showcase.gif)

## 🌍 Sobre o Projeto
Clone de Minecraft desenvolvido totalmente **do zero** em **C# e OpenGL**, sem uso de engines. O projeto implementa uma **pipeline de renderização completa** e geração procedural de mundos voxel-based, com foco em **arquitetura de código modular e desempenho**.

## ⚡ Funcionalidades e Conceitos Técnicos

- **Renderização 3D com OpenGL**  
  - Construção de **Vertex Buffers (VBOs)** e **Index Buffers (IBOs)** para representação eficiente de geometria.  
  - Implementação de **shaders customizados** (vertex e fragment shaders) com suporte a cores, texturas e iluminação básica.  
  - Transformações de objetos usando **matrizes de Model, View e Projection**, incluindo controle de câmera e perspectiva.  
  - Gerenciamento de **estado do pipeline OpenGL**, incluindo bindings, atributos de vértices e otimizações de draw calls.

- **Geração procedural de mundos voxel-based**  
  - Estruturação de chunks para renderização eficiente.  
  - Aplicação de algoritmos de **Perlin Noise e Simplex Noise** para variação de terreno.  
  - Otimização da atualização de chunks e culling de voxels invisíveis para desempenho em tempo real.

- **Arquitetura e estrutura de código**  
  - Código modular e desacoplado, permitindo fácil extensão e manutenção.  
  - Abstração de recursos gráficos (Shaders, Buffers, Textures) em classes específicas.  
  - Gerenciamento de memória e recursos GPU de forma eficiente, evitando vazamentos e redundâncias.  
  - Estrutura genérica para entidades 3D e sistemas de atualização/renderização, permitindo escalabilidade do projeto.  

- **Matemática aplicada à renderização**  
  - Operações com **vetores, matrizes e quaternions** para transformações e rotações.  
  - Implementação de projeção perspectiva e controle de câmera livre.  
  - Cálculos de colisão simples e detecção de visibilidade de chunks.

## 🛠 Tecnologias
- **C#**  
- **OpenGL**  
- Algoritmos de **Noise / Procedural Generation**
