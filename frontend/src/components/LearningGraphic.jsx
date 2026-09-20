import Icon from './Icon.jsx'

/** Decorative graph: no student metrics or progress are represented. */
export default function LearningGraphic() {
  return (
    <div className="learning-graphic" aria-hidden="true">
      <div className="orbit orbit-outer" />
      <div className="orbit orbit-inner" />
      <div className="orbit-axis orbit-axis-one" />
      <div className="orbit-axis orbit-axis-two" />
      <div className="orbit-core"><Icon name="code" /></div>
      <div className="orbit-node orbit-node-one"><Icon name="topics" /></div>
      <div className="orbit-node orbit-node-two"><Icon name="check" /></div>
      <div className="orbit-node orbit-node-three"><Icon name="spark" /></div>
      <span className="orbit-dot orbit-dot-one" />
      <span className="orbit-dot orbit-dot-two" />
    </div>
  )
}
