import type { PropsWithChildren } from "react";
import { createPortal } from "react-dom";
import styles from "./Modal.module.css";

export const Modal = ({ showing, close, children }: PropsWithChildren<{ showing: boolean, close: () => void }>) => {
  if (!showing) {
    return;
  }

  return (
    createPortal(
      <div className={styles.modalOverlay} onClick={close}>
        <div className={styles.clickContainer} onClick={e => e.stopPropagation()}>
          {children}
        </div>
      </div>,
      document.getElementById("root")!,
    ))
}