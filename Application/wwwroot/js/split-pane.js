class SplitPane extends HTMLElement {
    constructor() {
        super();

        const shadow = this.attachShadow({ mode: 'open' });

        shadow.innerHTML = `
      <style>
        :host {
          display: block;
          width: 100%;
          height: 100%;
        }

        .container {
          width: 100%;
          height: 100%;
          display: flex;
          overflow: hidden;
        }

        .pane {
          overflow: auto;
        }

        .divider {
          background: var(--color-bg-light);;
          flex-shrink: 0;
          position: relative;
          z-index: 10;
        }

        .divider:hover {
          background: var(--color-bg-light-hover);
        }

        .vertical {
          flex-direction: row;
        }

        .vertical .divider {
          width: 6px;
          cursor: col-resize;
        }

        .horizontal {
          flex-direction: column;
        }

        .horizontal .divider {
          height: 6px;
          cursor: row-resize;
        }
      </style>

      <div class="container">
        <div class="pane pane1">
          <slot name="first"></slot>
        </div>

        <div class="divider"></div>

        <div class="pane pane2">
          <slot name="second"></slot>
        </div>
      </div>
    `;
    }

    connectedCallback() {
        this.container = this.shadowRoot.querySelector('.container');
        this.pane1 = this.shadowRoot.querySelector('.pane1');
        this.pane2 = this.shadowRoot.querySelector('.pane2');
        this.divider = this.shadowRoot.querySelector('.divider');

        this.direction = this.getAttribute('direction') || 'vertical';
        this.initial = parseFloat(this.getAttribute('initial') || '50');
        this.minSize = parseInt(this.getAttribute('min-size') || '50', 10);

        this.container.classList.add(this.direction);

        this.setSplit(this.initial);

        this.divider.addEventListener('pointerdown', this.startDrag.bind(this));
    }

    setSplit(percent) {
        percent = Math.max(0, Math.min(100, percent));

        this.pane1.style.flexBasis = `${percent}%`;
        this.pane1.style.flexGrow = '0';

        this.pane2.style.flexBasis = `${100 - percent}%`;
        this.pane2.style.flexGrow = '0';
    }

    startDrag(e) {
        e.preventDefault();

        const move = (event) => {
            const rect = this.getBoundingClientRect();

            let percent;

            if (this.direction === 'vertical') {
                const x = event.clientX - rect.left;

                if (x < this.minSize ||
                    x > rect.width - this.minSize) {
                    return;
                }

                percent = (x / rect.width) * 100;
            } else {
                const y = event.clientY - rect.top;

                if (y < this.minSize ||
                    y > rect.height - this.minSize) {
                    return;
                }

                percent = (y / rect.height) * 100;
            }

            this.setSplit(percent);
        };

        const up = () => {
            window.removeEventListener('pointermove', move);
            window.removeEventListener('pointerup', up);
        };

        window.addEventListener('pointermove', move);
        window.addEventListener('pointerup', up);
    }
}

customElements.define('split-pane', SplitPane);