import { useParams } from "react-router-dom";
import styles from "./Posting.module.css";

export const Posting = () => {
  const { postingId } = useParams();
  return (<article className={styles.posting}>
    {postingId}
  </article>);
}